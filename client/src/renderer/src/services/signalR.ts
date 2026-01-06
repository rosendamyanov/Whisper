import * as signalR from '@microsoft/signalr'
import { StreamManager } from '../core/StreamManager'
import { VoiceManager } from '../core/VoiceManager'

type EventCallback = (...args: unknown[]) => void

class SignalRService {
  private chatConnection: signalR.HubConnection | null = null
  private callbacks: Record<string, EventCallback[]> = {}

  private streamConnection: signalR.HubConnection | null = null
  public streamManager: StreamManager | null = null

  private voiceConnection: signalR.HubConnection | null = null
  public voiceManager: VoiceManager | null = null

  public async connect(): Promise<void> {
    if (
      this.chatConnection?.state === signalR.HubConnectionState.Connected &&
      this.streamConnection?.state === signalR.HubConnectionState.Connected &&
      this.voiceConnection?.state === signalR.HubConnectionState.Connected
    ) {
      return
    }

    const token =
      localStorage.getItem('whisper-token') ||
      JSON.parse(localStorage.getItem('whisper-auth-storage') || '{}')?.state?.user?.token

    const apiUrl = import.meta.env.VITE_API_URL || 'https://localhost:7126/api'
    const rootUrl = apiUrl.replace(/\/api$/, '')
    const options = { accessTokenFactory: () => token || '' }

    this.chatConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${rootUrl}/hubs/chat`, options)
      .withAutomaticReconnect()
      .build()

    this.chatConnection.on('ReceiveMessage', (msg) => this.emit('ReceiveMessage', msg))
    this.chatConnection.on('ParticipantLeft', (data) => this.emit('ParticipantLeft', data))

    this.streamConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${rootUrl}/hubs/stream`, options)
      .withAutomaticReconnect()
      .build()

    this.streamManager = new StreamManager(this.streamConnection)

    this.voiceConnection = new signalR.HubConnectionBuilder()
      .withUrl(`${rootUrl}/hubs/voice`, options)
      .withAutomaticReconnect()
      .build()

    this.voiceManager = new VoiceManager(this.voiceConnection)

    try {
      await Promise.all([
        this.chatConnection.start(),
        this.streamConnection.start(),
        this.voiceConnection.start()
      ])
      console.log('✅ SignalR Connected: Chat + Stream + Voice ready.')
    } catch (err) {
      console.error('❌ SignalR Connection Error:', err)
    }

    this.chatConnection.onclose(() => console.log('Chat Disconnected'))
    this.streamConnection.onclose(() => console.log('Stream Disconnected'))
    this.voiceConnection.onclose(() => console.log('Voice Disconnected'))
  }

  public on(event: string, callback: EventCallback): void {
    if (!this.callbacks[event]) this.callbacks[event] = []
    this.callbacks[event].push(callback)
  }

  public off(event: string, callback: EventCallback): void {
    if (!this.callbacks[event]) return
    this.callbacks[event] = this.callbacks[event].filter((cb) => cb !== callback)
  }

  private emit(event: string, ...args: unknown[]): void {
    if (this.callbacks[event]) {
      this.callbacks[event].forEach((cb) => cb(...args))
    }
  }

  public async invoke(methodName: string, ...args: unknown[]): Promise<unknown> {
    if (this.chatConnection?.state === signalR.HubConnectionState.Connected) {
      return await this.chatConnection.invoke(methodName, ...args)
    }
    return Promise.reject(new Error('Chat connection is not connected.'))
  }

  public async joinChat(chatId: string): Promise<void> {
    await this.invoke('JoinChat', chatId)
  }

  public getChatConnection(): signalR.HubConnection | null {
    return this.chatConnection
  }
}

export const signalRService = new SignalRService()
