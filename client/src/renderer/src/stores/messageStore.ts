import { create } from 'zustand'
import { messageApi } from '../services/api/message'
import { signalRService } from '../services/signalR'
import { MessageResponse } from '../types/api/response/chat/MessageResponse'
import { UiMessage, mapMessageToUi } from '../utils/mappers/messageMapper'
import { useAuthStore } from './authStore'

interface MessageState {
  messages: UiMessage[]
  isLoading: boolean

  fetchMessages: (chatId: string) => Promise<void>
  sendMessage: (chatId: string, content: string) => Promise<void>
  addMessage: (message: MessageResponse) => void

  initializeListeners: () => void
}

export const useMessageStore = create<MessageState>((set, get) => ({
  messages: [],
  isLoading: false,

  fetchMessages: async (chatId) => {
    set({ isLoading: true, messages: [] })
    try {
      const res = await messageApi.getChatMessages(chatId)
      if (res.isSuccess && res.data) {
        const data = res.data as MessageResponse[] | { messages: MessageResponse[] }
        const rawList: MessageResponse[] = Array.isArray(data) ? data : data.messages || []
        const currentUserId = useAuthStore.getState().user?.id
        const uiList = rawList.map((msg) => mapMessageToUi(msg, currentUserId))
        const sorted = uiList.sort(
          (a, b) => new Date(a.sentAt).getTime() - new Date(b.sentAt).getTime()
        )

        set({ messages: sorted })
      }
      signalRService.joinChat(chatId)
    } catch (error) {
      console.error(error)
    } finally {
      set({ isLoading: false })
    }
  },

  sendMessage: async (chatId, content) => {
    try {
      await messageApi.sendMessage(chatId, content)
    } catch (error) {
      console.error(error)
    }
  },

  addMessage: (message: MessageResponse) => {
    set((state) => {
      if (state.messages.some((m) => m.id === message.id)) {
        return state
      }
      const currentUserId = useAuthStore.getState().user?.id
      const uiMessage = mapMessageToUi(message, currentUserId)
      return { messages: [...state.messages, uiMessage] }
    })
  },

  initializeListeners: () => {
    signalRService.on('ReceiveMessage', (msg: unknown): void => {
      get().addMessage(msg as MessageResponse)
    })
  }
}))
