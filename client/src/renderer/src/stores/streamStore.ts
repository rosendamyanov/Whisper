import { create } from 'zustand'
import { signalRService } from '../services/signalR'
import { ActiveStreamInfo } from '../core/StreamManager'

interface StreamState {
  isStreaming: boolean
  isWatching: boolean
  remoteStream: MediaStream | null
  activeChatId: string | null

  availableStreams: Record<string, ActiveStreamInfo>

  startSharing: (chatId: string) => Promise<void>
  stopSharing: () => void
  joinStream: (chatId: string) => Promise<void>
  leaveStream: () => void
  setQuality: (qualityKey: string) => Promise<void>
  monitorChat: (chatId: string) => Promise<void>

  setRemoteStream: (stream: MediaStream | null) => void
  addAvailableStream: (info: ActiveStreamInfo) => void
  removeAvailableStream: (chatId: string) => void
  setStreamingStatus: (isStreaming: boolean) => void
}

export const useStreamStore = create<StreamState>((set) => ({
  isStreaming: false,
  isWatching: false,
  remoteStream: null,
  activeChatId: null,
  availableStreams: {},

  setRemoteStream: (stream) => {
    set({ remoteStream: stream, isWatching: !!stream && stream.active })
  },

  addAvailableStream: (info) => {
    set((state) => ({
      availableStreams: { ...state.availableStreams, [info.chatId]: info }
    }))
  },

  removeAvailableStream: (chatId) => {
    set((state) => {
      const newStreams = { ...state.availableStreams }
      delete newStreams[chatId]
      return { availableStreams: newStreams }
    })
  },

  setStreamingStatus: (status) => {
    set({ isStreaming: status })
    if (!status) set({ activeChatId: null })
  },

  startSharing: async (chatId: string) => {
    if (!signalRService.streamManager) return
    try {
      const stream = await signalRService.streamManager.startScreenShare(chatId, '1080p_60')
      if (stream) {
        set({ isStreaming: true, activeChatId: chatId })
      }
    } catch (error) {
      console.error('Failed to start stream:', error)
    }
  },

  stopSharing: () => {
    signalRService.streamManager?.stopScreenShare()
  },

  joinStream: async (chatId: string) => {
    if (!signalRService.streamManager) return
    await signalRService.streamManager.joinStream(chatId)
    set({ activeChatId: chatId, isWatching: true })
  },

  leaveStream: () => {
    signalRService.streamManager?.leaveStream()
    set({ isWatching: false, remoteStream: null, activeChatId: null })
  },

  setQuality: async (qualityKey: string) => {
    await signalRService.streamManager?.setQuality(qualityKey)
  },
  monitorChat: async (chatId: string) => {
    if (!signalRService.streamManager) return

    // Reset state for the new chat first to avoid "ghost" buttons
    set(() => {
      // Optional: clear other chat streams if you only want to track the active one
      return { activeChatId: null, isWatching: false, isStreaming: false }
    })

    await signalRService.streamManager.monitorChat(chatId)
  }
}))

const initializeStreamListeners = (): void => {
  const checkInterval = setInterval(() => {
    if (signalRService.streamManager) {
      clearInterval(checkInterval)
      const manager = signalRService.streamManager

      manager.onRemoteStream = (stream) => {
        useStreamStore.getState().setRemoteStream(stream)
      }

      manager.onStreamStarted = (info) => {
        useStreamStore.getState().addAvailableStream(info)
      }

      manager.onStreamEnded = (chatId) => {
        useStreamStore.getState().removeAvailableStream(chatId)
      }

      manager.onStreamingStatusChanged = (isStreaming) => {
        useStreamStore.getState().setStreamingStatus(isStreaming)
      }
    }
  }, 500)
}

initializeStreamListeners()
