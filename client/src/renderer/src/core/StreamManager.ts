import { HubConnection } from '@microsoft/signalr'

export type StreamResolution = '720p' | '1080p' | '1440p' | '4k'
export type StreamFps = 30 | 60

export interface StreamQualitySettings {
  label: string
  width: number
  height: number
  fps: number
  maxBitrate: number // in bps
}

export const QUALITY_PRESETS: Record<string, StreamQualitySettings> = {
  '720p_30': { label: '720p 30fps', width: 1280, height: 720, fps: 30, maxBitrate: 6_000_000 }, // 6 Mbps
  '720p_60': { label: '720p 60fps', width: 1280, height: 720, fps: 60, maxBitrate: 9_000_000 }, // 9 Mbps
  '1080p_30': { label: '1080p 30fps', width: 1920, height: 1080, fps: 30, maxBitrate: 12_000_000 }, // 12 Mbps
  '1080p_60': { label: '1080p 60fps', width: 1920, height: 1080, fps: 60, maxBitrate: 18_000_000 }, // 18 Mbps
  '1440p_30': { label: '1440p 30fps', width: 2560, height: 1440, fps: 30, maxBitrate: 24_000_000 }, // 24 Mbps
  '1440p_60': { label: '1440p 60fps', width: 2560, height: 1440, fps: 60, maxBitrate: 35_000_000 }, // 35 Mbps
  '4k_30': { label: '4K 30fps', width: 3840, height: 2160, fps: 30, maxBitrate: 60_000_000 }, // 60 Mbps
  '4k_60': { label: '4K 60fps', width: 3840, height: 2160, fps: 60, maxBitrate: 100_000_000 } // 100 Mbps
}

export type QualityPresetKey = keyof typeof QUALITY_PRESETS
export const DEFAULT_QUALITY: QualityPresetKey = '1080p_60'

interface ViewerJoinedResponse {
  userId: string
  username: string
  viewerCount: number
}

export interface ActiveStreamInfo {
  chatId: string
  streamId: string
  hostUserId: string
  hostUsername: string
}

export class StreamManager {
  private localStream: MediaStream | null = null
  private viewers: Map<string, RTCPeerConnection> = new Map()
  private currentChatId: string | null = null
  private currentQualityKey: QualityPresetKey = DEFAULT_QUALITY
  private currentQuality: StreamQualitySettings = QUALITY_PRESETS[DEFAULT_QUALITY]
  public onStreamStarted: ((info: ActiveStreamInfo) => void) | null = null
  public onStreamEnded: ((chatId: string) => void) | null = null
  public onStreamingStatusChanged: ((isStreaming: boolean) => void) | null = null

  private rtcConfig: RTCConfiguration = {
    iceServers: [
      { urls: 'stun:stun.l.google.com:19302' },
      { urls: 'stun:stun1.l.google.com:19302' }
    ]
  }

  constructor(private signalR: HubConnection) {
    this.setupListeners()
  }

  public getQualityOptions(): { key: QualityPresetKey; settings: StreamQualitySettings }[] {
    return Object.entries(QUALITY_PRESETS).map(([key, settings]) => ({
      key: key as QualityPresetKey,
      settings
    }))
  }

  public getCurrentQualityKey(): QualityPresetKey {
    return this.currentQualityKey
  }

  public getCurrentQuality(): StreamQualitySettings {
    return this.currentQuality
  }

  public getCurrentChatId(): string | null {
    return this.currentChatId
  }

  public isStreaming(): boolean {
    return this.localStream !== null
  }

  public getLocalStream(): MediaStream | null {
    return this.localStream
  }
  public onRemoteStream: ((stream: MediaStream) => void) | null = null

  private hostConnection: RTCPeerConnection | null = null

  public async startScreenShare(
    chatId: string,
    qualityKey: QualityPresetKey = DEFAULT_QUALITY
  ): Promise<MediaStream | null> {
    this.currentChatId = chatId
    this.currentQualityKey = qualityKey
    this.currentQuality = QUALITY_PRESETS[qualityKey]
    try {
      this.localStream = await navigator.mediaDevices.getDisplayMedia({
        video: {
          width: { ideal: this.currentQuality.width },
          height: { ideal: this.currentQuality.height },
          frameRate: { ideal: this.currentQuality.fps }
        },
        audio: {
          echoCancellation: true,
          noiseSuppression: true
        }
      })

      this.localStream.getVideoTracks()[0].onended = () => {
        this.stopScreenShare()
      }

      await this.signalR.invoke('StartStream', chatId)

      console.log(
        `Screen share started in chat ${chatId} with quality: ${this.currentQuality.label}`
      )
      return this.localStream
    } catch (error) {
      console.error('Failed to start screen share:', error)
      this.currentChatId = null
      return null
    }
  }

  public async setQuality(qualityKey: QualityPresetKey): Promise<boolean> {
    if (!QUALITY_PRESETS[qualityKey]) return false

    this.currentQualityKey = qualityKey
    this.currentQuality = QUALITY_PRESETS[qualityKey]

    if (this.localStream) {
      const videoTrack = this.localStream.getVideoTracks()[0]
      if (videoTrack) {
        try {
          await videoTrack.applyConstraints({
            width: { ideal: this.currentQuality.width },
            height: { ideal: this.currentQuality.height },
            frameRate: { ideal: this.currentQuality.fps }
          })
        } catch (error) {
          console.error('Failed to apply quality constraints:', error)
          return false
        }
      }

      const updates = Array.from(this.viewers.values()).map((pc) => {
        const videoSender = pc.getSenders().find((s) => s.track?.kind === 'video')
        if (videoSender) {
          return this.updateSenderBitrate(videoSender, this.currentQuality.maxBitrate)
        }
        return Promise.resolve()
      })
      await Promise.all(updates)
    }
    return true
  }

  public stopScreenShare(): void {
    if (this.localStream) {
      this.localStream.getTracks().forEach((track) => track.stop())
      this.localStream = null
    }

    if (this.currentChatId) {
      this.signalR.invoke('EndStream', this.currentChatId).catch(() => {})
    }

    this.viewers.forEach((pc) => pc.close())
    this.viewers.clear()

    if (this.hostConnection) {
      this.hostConnection.close()
      this.hostConnection = null
    }

    this.currentChatId = null

    if (this.onRemoteStream) {
      this.onRemoteStream(new MediaStream())
    }

    if (this.onStreamingStatusChanged) {
      this.onStreamingStatusChanged(false)
    }

    console.log('Screen share stopped')
  }

  public async joinStream(chatid: string): Promise<void> {
    this.currentChatId = chatid
    try {
      await this.signalR.invoke('JoinStream', chatid)
    } catch (error) {
      console.error('Failed to join stream:', error)
    }
  }

  public leaveStream(): void {
    if (this.currentChatId) {
      this.signalR.invoke('LeaveStream', this.currentChatId).catch(console.error)
    }

    if (this.hostConnection) {
      this.hostConnection.close()
      this.hostConnection = null
    }

    this.currentChatId = null
    if (this.onRemoteStream) {
      this.onRemoteStream(new MediaStream())
    }
  }

  public async monitorChat(chatId: string): Promise<ActiveStreamInfo | null> {
    try {
      // 1. Join the SignalR Group for this chat on the STREAM hub
      // (This ensures we get future 'StreamStarted' events)
      await this.signalR.invoke('JoinStreamGroup', chatId)

      // 2. Ask the server: "Is anyone streaming right now?"
      // (This fixes the refresh issue)
      const currentStream = await this.signalR.invoke<ActiveStreamInfo | null>(
        'GetStreamStatus',
        chatId
      )

      if (currentStream) {
        // Manually trigger the event so our store updates
        if (this.onStreamStarted) {
          this.onStreamStarted(currentStream)
        }
      } else {
        // Ensure UI is clear if no stream
        if (this.onStreamEnded) {
          this.onStreamEnded(chatId)
        }
      }

      return currentStream
    } catch (error) {
      console.error('Failed to monitor chat stream:', error)
      return null
    }
  }

  private async updateSenderBitrate(sender: RTCRtpSender, bitrate: number): Promise<void> {
    const params = sender.getParameters()

    if (!params.encodings) params.encodings = [{}]

    params.encodings[0].maxBitrate = bitrate
    params.encodings[0].networkPriority = 'high'
    // @ts-ignore -- 'degradationPreference' is valid in standard WebRTC but missing in TypeScript lib.dom.d.ts
    params.encodings[0].degradationPreference = 'maintain-framerate'

    try {
      await sender.setParameters(params)
    } catch (error) {
      console.warn('Failed to set bitrate parameters:', error)
    }
  }

  private async connectToViewer(viewerId: string): Promise<void> {
    if (!this.localStream || !this.currentChatId) return

    const pc = new RTCPeerConnection(this.rtcConfig)
    this.viewers.set(viewerId, pc)

    this.localStream.getTracks().forEach((track) => {
      const sender = pc.addTrack(track, this.localStream!)

      if (track.kind === 'video') {
        this.updateSenderBitrate(sender, this.currentQuality.maxBitrate)
      }
    })

    pc.onicecandidate = (event) => {
      if (event.candidate && this.currentChatId) {
        this.signalR
          .invoke('SendIceCandidate', this.currentChatId, viewerId, JSON.stringify(event.candidate))
          .catch(console.error)
      }
    }

    pc.onconnectionstatechange = () => {
      if (pc.connectionState === 'failed' || pc.connectionState === 'disconnected') {
        this.closeViewerConnection(viewerId)
      }
    }

    try {
      const offer = await pc.createOffer()
      await pc.setLocalDescription(offer)
      await this.signalR.invoke('SendOffer', this.currentChatId, viewerId, JSON.stringify(offer))
    } catch (error) {
      console.error(`Failed to connect to viewer ${viewerId}:`, error)
      this.closeViewerConnection(viewerId)
    }
  }
  private closeViewerConnection(viewerId: string): void {
    const pc = this.viewers.get(viewerId)
    if (pc) {
      pc.close()
      this.viewers.delete(viewerId)
      console.log(`Connection to viewer ${viewerId} closed`)
    }
  }

  private setupListeners(): void {
    this.signalR.on('ViewerJoined', async (data: ViewerJoinedResponse) => {
      console.log(`Viewer ${data.username} joined. Initiating connection...`)
      await this.connectToViewer(data.userId)
    })

    this.signalR.on('ReceiveAnswer', async (data: { fromUserId: string; answer: string }) => {
      const pc = this.viewers.get(data.fromUserId)
      if (pc) {
        await pc.setRemoteDescription(JSON.parse(data.answer))
      }
    })

    this.signalR.on(
      'ReceiveIceCandidate',
      async (data: { fromUserId: string; candidate: string }) => {
        const candidate = JSON.parse(data.candidate)

        const viewerPc = this.viewers.get(data.fromUserId)
        if (viewerPc) {
          await viewerPc.addIceCandidate(candidate)
          return
        }

        if (this.hostConnection) {
          await this.hostConnection.addIceCandidate(candidate)
        }
      }
    )

    this.signalR.on('ReceiveOffer', async (data: { fromUserId: string; offer: string }) => {
      console.log('Received Offer from Host. Accepting...')

      const pc = new RTCPeerConnection(this.rtcConfig)
      this.hostConnection = pc

      pc.ontrack = (event) => {
        if (event.streams && event.streams[0]) {
          console.log('Remote stream received!')
          if (this.onRemoteStream) {
            this.onRemoteStream(event.streams[0])
          }
        }
      }

      pc.onicecandidate = (event) => {
        if (event.candidate && this.currentChatId) {
          this.signalR
            .invoke(
              'SendIceCandidate',
              this.currentChatId,
              data.fromUserId,
              JSON.stringify(event.candidate)
            )
            .catch(console.error)
        }
      }

      try {
        await pc.setRemoteDescription(JSON.parse(data.offer))
        const answer = await pc.createAnswer()
        await pc.setLocalDescription(answer)
        await this.signalR.invoke('SendAnswer', this.currentChatId, JSON.stringify(answer))
      } catch (err) {
        console.error('Error accepting stream offer:', err)
      }
    })

    this.signalR.on('StreamStarted', (info: ActiveStreamInfo) => {
      console.log(`Stream started in chat ${info.chatId}`)
      if (this.onStreamStarted) {
        this.onStreamStarted(info)
      }
    })

    this.signalR.on('ViewerLeft', (data: { userId: string }) => {
      this.closeViewerConnection(data.userId)
    })

    this.signalR.on('StreamEnded', (data: { chatId: string }) => {
      if (this.onStreamEnded) {
        this.onStreamEnded(data.chatId)
      }

      if (this.currentChatId === data.chatId) {
        this.stopScreenShare()
      }
    })
  }
}
