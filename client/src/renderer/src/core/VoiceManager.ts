import { HubConnection } from '@microsoft/signalr'

interface Peer {
  connection: RTCPeerConnection
  userId: string
}

interface SessionParticipant {
  userId: string
  username: string
  isMuted?: boolean
  isDeafened?: boolean
  isSpeaking?: boolean
}

interface SessionState {
  sessionId: string
  participants: Record<string, SessionParticipant>
}

export class VoiceManager {
  private signalR: HubConnection
  private localStream: MediaStream | null = null
  private peers: Map<string, Peer> = new Map()
  private currentChatId: string | null = null
  private currentUserId: string | null = null

  private audioContext: AudioContext | null = null
  private analyser: AnalyserNode | null = null
  private speakingInterval: number | null = null
  private isSpeaking = false
  private speechThreshold = 15

  public onRemoteStream?: (userId: string, stream: MediaStream) => void
  public onParticipantLeft?: (userId: string) => void

  private rtcConfig: RTCConfiguration = {
    iceServers: [
      { urls: 'stun:stun.l.google.com:19302' },
      { urls: 'stun:stun1.l.google.com:19302' }
    ]
  }

  constructor(signalRConnection: HubConnection) {
    this.signalR = signalRConnection
    this.setupSignalRListeners()
  }

  private setupSignalRListeners(): void {
    this.signalR.on('ReceiveOffer', async (data: { fromUserId: string; offer: string }) => {
      await this.handleOffer(data.fromUserId, JSON.parse(data.offer))
    })

    this.signalR.on('ReceiveAnswer', async (data: { fromUserId: string; answer: string }) => {
      await this.handleAnswer(data.fromUserId, JSON.parse(data.answer))
    })

    this.signalR.on(
      'ReceiveIceCandidate',
      async (data: { fromUserId: string; candidate: string }) => {
        await this.handleIceCandidate(data.fromUserId, JSON.parse(data.candidate))
      }
    )

    this.signalR.on('ParticipantLeft', (data: { userId: string }) => {
      this.closePeerConnection(data.userId)
      if (this.onParticipantLeft) this.onParticipantLeft(data.userId)
    })
  }

  public async joinSession(chatId: string, currentUserId: string): Promise<SessionState | null> {
    this.currentChatId = chatId
    this.currentUserId = currentUserId

    try {
      this.localStream = await navigator.mediaDevices.getUserMedia({
        audio: {
          echoCancellation: true,
          noiseSuppression: true,
          autoGainControl: true
        },
        video: false
      })

      this.startSpeakingDetection()
    } catch (err) {
      console.error('Failed to access microphone:', err)
      throw err
    }

    const sessionState = await this.signalR.invoke<SessionState>('JoinOrCreateSession', chatId)
    if (!sessionState) return null

    const participants = sessionState.participants || {}
    Object.values(participants).forEach((p: SessionParticipant) => {
      if (p.userId !== this.currentUserId) {
        const shouldInitiate = this.shouldInitiateConnection(this.currentUserId!, p.userId)
        this.createPeerConnection(p.userId, shouldInitiate)
      }
    })

    return sessionState
  }

  public leaveSession(): void {
    if (this.currentChatId && this.signalR.state === 'Connected') {
      this.signalR.invoke('LeaveSession', this.currentChatId).catch((err) => {
        console.error('Failed to leave session:', err)
      })
    }

    this.stopSpeakingDetection()

    this.localStream?.getTracks().forEach((track) => {
      track.stop()
      track.enabled = false
    })
    this.localStream = null

    this.peers.forEach((peer) => {
      try {
        peer.connection.close()
      } catch (err) {
        console.error('Error closing peer connection:', err)
      }
    })
    this.peers.clear()

    this.currentChatId = null
    this.currentUserId = null
  }

  public toggleMute(isMuted: boolean): void {
    if (this.localStream) {
      this.localStream.getAudioTracks().forEach((track) => (track.enabled = !isMuted))
    }
    if (this.currentChatId) {
      this.signalR.invoke('SetMute', this.currentChatId, isMuted).catch((err) => {
        console.error('Failed to toggle mute:', err)
      })
    }
  }

  public toggleDeafen(isDeafened: boolean): void {
    this.peers.forEach((peer) => {
      peer.connection.getReceivers().forEach((receiver) => {
        if (receiver.track) receiver.track.enabled = !isDeafened
      })
    })

    if (isDeafened) {
      this.toggleMute(true)
    }

    if (this.currentChatId) {
      this.signalR.invoke('ToggleDeafen', this.currentChatId).catch((err) => {
        console.error('Failed to toggle deafen:', err)
      })
    }
  }

  public setSpeechThreshold(threshold: number): void {
    this.speechThreshold = Math.max(0, Math.min(255, threshold))
  }

  private startSpeakingDetection(): void {
    if (!this.localStream) return

    this.audioContext = new AudioContext()
    const source = this.audioContext.createMediaStreamSource(this.localStream)
    this.analyser = this.audioContext.createAnalyser()
    this.analyser.fftSize = 512
    this.analyser.smoothingTimeConstant = 0.8
    source.connect(this.analyser)

    const dataArray = new Uint8Array(this.analyser.frequencyBinCount)

    this.speakingInterval = window.setInterval(() => {
      if (!this.analyser || !this.currentChatId) return

      this.analyser.getByteFrequencyData(dataArray)

      let sum = 0
      for (let i = 0; i < dataArray.length; i++) {
        sum += dataArray[i]
      }
      const average = sum / dataArray.length

      const nowSpeaking = average > this.speechThreshold

      if (nowSpeaking !== this.isSpeaking) {
        this.isSpeaking = nowSpeaking
        const method = nowSpeaking ? 'StartSpeaking' : 'StopSpeaking'

        this.signalR.invoke(method, this.currentChatId).catch((err) => {
          console.error('Failed to update speaking state:', err)
        })
      }
    }, 100) as number
  }

  private stopSpeakingDetection(): void {
    if (this.speakingInterval !== null) {
      clearInterval(this.speakingInterval)
      this.speakingInterval = null
    }

    if (this.audioContext) {
      this.audioContext.close().catch((err) => {
        console.error('Failed to close audio context:', err)
      })
      this.audioContext = null
    }

    this.analyser = null
    this.isSpeaking = false
  }

  public cleanup(): void {
    this.signalR.off('ReceiveOffer')
    this.signalR.off('ReceiveAnswer')
    this.signalR.off('ReceiveIceCandidate')
    this.signalR.off('ParticipantLeft')

    this.leaveSession()
  }

  private shouldInitiateConnection(myUserId: string, theirUserId: string): boolean {
    return myUserId.localeCompare(theirUserId) < 0
  }

  private createPeerConnection(
    targetUserId: string,
    isInitiator: boolean
  ): RTCPeerConnection | null {
    if (this.peers.has(targetUserId)) {
      return this.peers.get(targetUserId)!.connection
    }

    const connection = new RTCPeerConnection(this.rtcConfig)
    this.peers.set(targetUserId, { connection, userId: targetUserId })

    if (this.localStream) {
      this.localStream.getTracks().forEach((track) => {
        connection.addTrack(track, this.localStream!)
      })
    }

    connection.onicecandidate = (event) => {
      if (event.candidate && this.currentChatId) {
        this.signalR
          .invoke(
            'SendIceCandidate',
            this.currentChatId,
            targetUserId,
            JSON.stringify(event.candidate)
          )
          .catch((err) => {
            console.error('Failed to send ICE candidate:', err)
          })
      }
    }

    connection.ontrack = (event) => {
      if (event.streams && event.streams[0]) {
        if (this.onRemoteStream) {
          this.onRemoteStream(targetUserId, event.streams[0])
        }
      }
    }

    connection.onconnectionstatechange = () => {
      console.log(`Connection with ${targetUserId}: ${connection.connectionState}`)

      if (connection.connectionState === 'failed') {
        console.error(`Connection failed with ${targetUserId}, attempting to reconnect...`)
      }
    }

    connection.oniceconnectionstatechange = () => {
      console.log(`ICE connection with ${targetUserId}: ${connection.iceConnectionState}`)
    }

    if (isInitiator) {
      this.createOffer(targetUserId, connection)
    }

    return connection
  }

  private async createOffer(targetUserId: string, connection: RTCPeerConnection): Promise<void> {
    try {
      const offer = await connection.createOffer()
      await connection.setLocalDescription(offer)

      if (this.currentChatId) {
        await this.signalR.invoke(
          'SendOffer',
          this.currentChatId,
          targetUserId,
          JSON.stringify(offer)
        )
      }
    } catch (err) {
      console.error(`Failed to create offer for ${targetUserId}:`, err)
    }
  }

  private async handleOffer(fromUserId: string, offer: RTCSessionDescriptionInit): Promise<void> {
    const connection = this.createPeerConnection(fromUserId, false)
    if (!connection) return

    try {
      await connection.setRemoteDescription(new RTCSessionDescription(offer))
      const answer = await connection.createAnswer()
      await connection.setLocalDescription(answer)

      if (this.currentChatId) {
        await this.signalR.invoke(
          'SendAnswer',
          this.currentChatId,
          fromUserId,
          JSON.stringify(answer)
        )
      }
    } catch (err) {
      console.error(`Failed to handle offer from ${fromUserId}:`, err)
    }
  }

  private async handleAnswer(fromUserId: string, answer: RTCSessionDescriptionInit): Promise<void> {
    const peer = this.peers.get(fromUserId)
    if (!peer) {
      console.warn(`Received answer from unknown peer: ${fromUserId}`)
      return
    }

    try {
      await peer.connection.setRemoteDescription(new RTCSessionDescription(answer))
    } catch (err) {
      console.error(`Failed to handle answer from ${fromUserId}:`, err)
    }
  }

  private async handleIceCandidate(
    fromUserId: string,
    candidate: RTCIceCandidateInit
  ): Promise<void> {
    const peer = this.peers.get(fromUserId)
    if (!peer) {
      console.warn(`Received ICE candidate from unknown peer: ${fromUserId}`)
      return
    }

    try {
      await peer.connection.addIceCandidate(new RTCIceCandidate(candidate))
    } catch (err) {
      console.error(`Failed to add ICE candidate from ${fromUserId}:`, err)
    }
  }

  private closePeerConnection(userId: string): void {
    const peer = this.peers.get(userId)
    if (peer) {
      peer.connection.close()
      this.peers.delete(userId)
    }
  }
}
