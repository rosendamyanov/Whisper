import { HubConnection } from "@microsoft/signalr";

interface Peer {
    connection: RTCPeerConnection;
    userId: string;
}

export class VoiceManager {
    private signalR: HubConnection;
    private localStream: MediaStream | null = null;
    private peers: Map<string, Peer> = new Map();
    private currentChatId: string | null = null;
    
    // Audio Analysis for "Speaking" detection
    private audioContext: AudioContext | null = null;
    private analyser: AnalyserNode | null = null;
    private speakingInterval: any = null;
    private isSpeaking = false;
    private speechThreshold = 15; // Volume threshold (0-255)

    public onRemoteStream?: (userId: string, stream: MediaStream) => void;
    public onParticipantLeft?: (userId: string) => void;

    private rtcConfig: RTCConfiguration = {
        iceServers: [
            { urls: "stun:stun.l.google.com:19302" }
        ]
    };

    constructor(signalRConnection: HubConnection) {
        this.signalR = signalRConnection;
        this.setupSignalRListeners();
    }

    private setupSignalRListeners() {
        this.signalR.on("ReceiveOffer", async (data: { fromUserId: string, offer: string }) => {
            await this.handleOffer(data.fromUserId, JSON.parse(data.offer));
        });

        this.signalR.on("ReceiveAnswer", async (data: { fromUserId: string, answer: string }) => {
            await this.handleAnswer(data.fromUserId, JSON.parse(data.answer));
        });

        this.signalR.on("ReceiveIceCandidate", async (data: { fromUserId: string, candidate: string }) => {
            await this.handleIceCandidate(data.fromUserId, JSON.parse(data.candidate));
        });

        this.signalR.on("ParticipantLeft", (data: { userId: string }) => {
            this.closePeerConnection(data.userId);
            if (this.onParticipantLeft) this.onParticipantLeft(data.userId);
        });
    }

    public async joinSession(chatId: string, currentUserId: string): Promise<any> {
        this.currentChatId = chatId;

        try {
            this.localStream = await navigator.mediaDevices.getUserMedia({
                audio: { echoCancellation: true, noiseSuppression: true, autoGainControl: true },
                video: false
            });

            // --- START SPEAKING DETECTION ---
            this.startSpeakingDetection();

        } catch (err) {
            console.error("Failed to access microphone", err);
            throw err;
        }

        const sessionState = await this.signalR.invoke("JoinOrCreateSession", chatId);
        if (!sessionState) return null;

        const participants = sessionState.participants || {};
        Object.values(participants).forEach((p: any) => {
            if (p.userId !== currentUserId) {
                this.createPeerConnection(p.userId, true);
            }
        });

        return sessionState;
    }

    public leaveSession() {
        if (this.currentChatId && this.signalR.state === "Connected") {
            this.signalR.invoke("LeaveSession", this.currentChatId).catch(() => {});
        }
        
        // Stop Audio Analysis
        this.stopSpeakingDetection();

        // Stop Mic Tracks
        this.localStream?.getTracks().forEach(track => {
            track.stop();
            track.enabled = false;
        });
        this.localStream = null;

        // Close Peers
        this.peers.forEach(peer => {
            try { peer.connection.close(); } catch (e) { /* ignore */ }
        });
        this.peers.clear();
        this.currentChatId = null;
    }

    // --- MUTE / DEAFEN ---

    public toggleMute(isMuted: boolean) {
        if (this.localStream) {
            this.localStream.getAudioTracks().forEach(track => track.enabled = !isMuted);
        }
        if (this.currentChatId) {
            // Server expects "SetMute", not toggle
            this.signalR.invoke("SetMute", this.currentChatId, isMuted).catch(console.error);
        }
    }

    public toggleDeafen(isDeafened: boolean) {
        // "Deafen" means we can't hear others
        this.peers.forEach(peer => {
            peer.connection.getReceivers().forEach(receiver => {
                if(receiver.track) receiver.track.enabled = !isDeafened;
            });
        });
        
        // Also mute ourselves if deafened (standard logic)
        if (isDeafened) {
            this.toggleMute(true);
        } else {
            // Restore mic if we undeafen (optional, usually we stay muted if we were muted before)
            // For simplicity, let's just sync with backend
        }

        if (this.currentChatId) {
            this.signalR.invoke("ToggleDeafen", this.currentChatId).catch(console.error);
        }
    }

    // --- SPEAKING DETECTION LOGIC ---

    private startSpeakingDetection() {
        if (!this.localStream) return;

        // 1. Setup Audio Context
        this.audioContext = new AudioContext();
        const source = this.audioContext.createMediaStreamSource(this.localStream);
        this.analyser = this.audioContext.createAnalyser();
        this.analyser.fftSize = 512;
        source.connect(this.analyser);

        const dataArray = new Uint8Array(this.analyser.frequencyBinCount);

        // 2. Poll volume every 100ms
        this.speakingInterval = setInterval(() => {
            if (!this.analyser || !this.currentChatId) return;

            this.analyser.getByteFrequencyData(dataArray);

            // Calculate average volume
            let sum = 0;
            for (let i = 0; i < dataArray.length; i++) {
                sum += dataArray[i];
            }
            const average = sum / dataArray.length;

            // 3. Determine state
            const nowSpeaking = average > this.speechThreshold;

            if (nowSpeaking !== this.isSpeaking) {
                this.isSpeaking = nowSpeaking;
                const method = nowSpeaking ? "StartSpeaking" : "StopSpeaking";
                
                // Invoke backend (Fire & Forget)
                this.signalR.invoke(method, this.currentChatId).catch(() => {});
            }

        }, 100);
    }

    private stopSpeakingDetection() {
        if (this.speakingInterval) clearInterval(this.speakingInterval);
        if (this.audioContext) this.audioContext.close();
        this.speakingInterval = null;
        this.audioContext = null;
        this.analyser = null;
        this.isSpeaking = false;
    }

    // --- CLEANUP ---
    public cleanup() {
        this.signalR.off("ReceiveOffer");
        this.signalR.off("ReceiveAnswer");
        this.signalR.off("ReceiveIceCandidate");
        this.signalR.off("ParticipantLeft");
        
        this.leaveSession();
    }

    // ... (WebRTC createPeerConnection/offer/answer logic remains exactly the same) ...
    // Paste your existing WebRTC logic here
    // ...
    private createPeerConnection(targetUserId: string, isInitiator: boolean) {
        if (this.peers.has(targetUserId)) return this.peers.get(targetUserId)!.connection;

        const connection = new RTCPeerConnection(this.rtcConfig);
        this.peers.set(targetUserId, { connection, userId: targetUserId });

        if (this.localStream) {
            this.localStream.getTracks().forEach(track => {
                connection.addTrack(track, this.localStream!);
            });
        }

        connection.onicecandidate = (event) => {
            if (event.candidate && this.currentChatId) {
                this.signalR.invoke("SendIceCandidate", this.currentChatId, targetUserId, JSON.stringify(event.candidate));
            }
        };

        connection.ontrack = (event) => {
            if (event.streams && event.streams[0]) {
                if (this.onRemoteStream) {
                    this.onRemoteStream(targetUserId, event.streams[0]);
                }
            }
        };

        if (isInitiator) {
            this.createOffer(targetUserId, connection);
        }

        return connection;
    }

    private async createOffer(targetUserId: string, connection: RTCPeerConnection) {
        try {
            const offer = await connection.createOffer();
            await connection.setLocalDescription(offer);
            if (this.currentChatId) {
                await this.signalR.invoke("SendOffer", this.currentChatId, targetUserId, JSON.stringify(offer));
            }
        } catch (err) { console.error(err); }
    }

    private async handleOffer(fromUserId: string, offer: RTCSessionDescriptionInit) {
        const connection = this.createPeerConnection(fromUserId, false);
        if (!connection) return;
        try {
            await connection.setRemoteDescription(new RTCSessionDescription(offer));
            const answer = await connection.createAnswer();
            await connection.setLocalDescription(answer);
            if (this.currentChatId) {
                await this.signalR.invoke("SendAnswer", this.currentChatId, fromUserId, JSON.stringify(answer));
            }
        } catch (err) { console.error(err); }
    }

    private async handleAnswer(fromUserId: string, answer: RTCSessionDescriptionInit) {
        const peer = this.peers.get(fromUserId);
        if (peer) {
            await peer.connection.setRemoteDescription(new RTCSessionDescription(answer));
        }
    }

    private async handleIceCandidate(fromUserId: string, candidate: RTCIceCandidateInit) {
        const peer = this.peers.get(fromUserId);
        if (peer) {
            await peer.connection.addIceCandidate(new RTCIceCandidate(candidate));
        }
    }
    
    private closePeerConnection(userId: string) {
        const peer = this.peers.get(userId);
        if (peer) {
            peer.connection.close();
            this.peers.delete(userId);
        }
    }
}