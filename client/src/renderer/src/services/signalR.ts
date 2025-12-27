import * as signalR from '@microsoft/signalr';

class SignalRService {
    private connection: signalR.HubConnection | null = null;
    private callbacks: Record<string, ((...args: any[]) => void)[]> = {};

    public async connect() {
        if (this.connection?.state === signalR.HubConnectionState.Connected) return;

        const token = localStorage.getItem('whisper-token') || 
                      JSON.parse(localStorage.getItem('whisper-auth-storage') || '{}')?.state?.user?.token; 

        // 1. Get the API URL from env or fallback
        const apiUrl = import.meta.env.VITE_API_URL || 'https://localhost:7126/api';
        
        // 2. Remove '/api' from the end to get the root URL (e.g., https://192.168.1.15:7126)
        const rootUrl = apiUrl.replace(/\/api$/, '');

        // 3. Append the Hub path
        const hubUrl = `${rootUrl}/hubs/chat`;

        this.connection = new signalR.HubConnectionBuilder()
            .withUrl(hubUrl, {  // <--- UPDATED HERE
                accessTokenFactory: () => token || ''
            })
            .withAutomaticReconnect()
            .build();

        this.connection.onclose(() => console.log('SignalR Disconnected'));

        this.connection.on('ReceiveMessage', (message) => {
            this.emit('ReceiveMessage', message);
        });
        
        // --- LISTENERS FOR VOICE/WebRTC ---
        this.connection.on('ReceiveOffer', (data) => this.emit('ReceiveOffer', data));
        this.connection.on('ReceiveAnswer', (data) => this.emit('ReceiveAnswer', data));
        this.connection.on('ReceiveIceCandidate', (data) => this.emit('ReceiveIceCandidate', data));
        this.connection.on('ParticipantLeft', (data) => this.emit('ParticipantLeft', data));

        try {
            await this.connection.start();
            console.log('SignalR Connected');
        } catch (err) {
            console.error('SignalR Connection Error: ', err);
        }
    }

    public on(event: string, callback: (...args: any[]) => void) {
        if (!this.callbacks[event]) this.callbacks[event] = [];
        this.callbacks[event].push(callback);
        
        // If we are proxying events from the raw connection (like for VoiceManager),
        // we might need to attach them here if they weren't attached in connect().
        // However, VoiceManager usually attaches its own listeners directly to the connection object.
    }

    public off(event: string, callback: (...args: any[]) => void) {
        if (!this.callbacks[event]) return;
        this.callbacks[event] = this.callbacks[event].filter(cb => cb !== callback);
    }

    private emit(event: string, ...args: any[]) {
        if (this.callbacks[event]) {
            this.callbacks[event].forEach(cb => cb(...args));
        }
    }

    public async invoke(methodName: string, ...args: any[]) {
        if (this.connection?.state === signalR.HubConnectionState.Connected) {
            return await this.connection.invoke(methodName, ...args);
        }
    }

    public async joinChat(chatId: string) {
        await this.invoke('JoinChat', chatId);
    }

    // --- ADD THIS METHOD ---
    public getConnection(): signalR.HubConnection {
        if (!this.connection) {
            throw new Error("SignalR connection has not been initialized. Call connect() first.");
        }
        return this.connection;
    }
}

export const signalRService = new SignalRService();