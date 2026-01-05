import * as signalR from '@microsoft/signalr'

type EventCallback = (...args: unknown[]) => void

class SignalRService {
  private connection: signalR.HubConnection | null = null
  private callbacks: Record<string, EventCallback[]> = {}

  public async connect(): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) return

    const token =
      localStorage.getItem('whisper-token') ||
      JSON.parse(localStorage.getItem('whisper-auth-storage') || '{}')?.state?.user?.token

    const apiUrl = import.meta.env.VITE_API_URL || 'https://localhost:7126/api'

    const rootUrl = apiUrl.replace(/\/api$/, '')

    const hubUrl = `${rootUrl}/hubs/chat`

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => token || ''
      })
      .withAutomaticReconnect()
      .build()

    this.connection.onclose(() => console.log('SignalR Disconnected'))

    this.connection.on('ReceiveMessage', (message) => {
      this.emit('ReceiveMessage', message)
    })

    this.connection.on('ReceiveOffer', (data) => this.emit('ReceiveOffer', data))
    this.connection.on('ReceiveAnswer', (data) => this.emit('ReceiveAnswer', data))
    this.connection.on('ReceiveIceCandidate', (data) => this.emit('ReceiveIceCandidate', data))
    this.connection.on('ParticipantLeft', (data) => this.emit('ParticipantLeft', data))

    try {
      await this.connection.start()
      console.log('SignalR Connected')
    } catch (err) {
      console.error('SignalR Connection Error: ', err)
    }
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
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      return await this.connection.invoke(methodName, ...args)
    }
    return Promise.reject(new Error('SignalR connection is not connected.'))
  }

  public async joinChat(chatId: string): Promise<void> {
    await this.invoke('JoinChat', chatId)
  }

  public getConnection(): signalR.HubConnection {
    if (!this.connection) {
      throw new Error('SignalR connection has not been initialized. Call connect() first.')
    }
    return this.connection
  }
}

export const signalRService = new SignalRService()
