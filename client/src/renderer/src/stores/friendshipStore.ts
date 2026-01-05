import { create } from 'zustand'
import { usersApi } from '../services/api/user'
import { friendshipApi } from '../services/api/friendship'
import { User } from '../types/api/response/user/User'
import { chatApi } from '../services/api/chat'
import { useChatStore } from './chatStore'
import { useAuthStore } from './authStore'
import { FriendRequest } from '@renderer/types/api/request/friendship/FriendRequest'

interface FriendState {
  friends: User[]
  searchResults: User[]
  pendingRequests: FriendRequest[]
  isLoading: boolean

  searchUsers: (query: string) => Promise<void>
  sendFriendRequest: (userId: string) => Promise<void>
  fetchFriends: () => Promise<void>
  fetchPendingRequests: () => Promise<void>
  acceptRequest: (friendshipId: string) => Promise<void>
  declineRequest: (friendshipId: string) => Promise<void>
  clearSearch: () => void
}

export const useFriendStore = create<FriendState>((set, get) => ({
  friends: [],
  searchResults: [],
  pendingRequests: [],
  isLoading: false,

  searchUsers: async (query) => {
    if (!query || query.length < 3) return
    set({ isLoading: true })
    try {
      const res = await usersApi.search(query)
      if (res.isSuccess && res.data) {
        set({ searchResults: res.data })
      } else {
        set({ searchResults: [] })
      }
    } catch (error) {
      console.error(error)
      set({ searchResults: [] })
    } finally {
      set({ isLoading: false })
    }
  },

  sendFriendRequest: async (userId) => {
    try {
      await friendshipApi.sendRequest(userId)
    } catch (error) {
      console.error('Failed to send request', error)
    }
  },

  fetchFriends: async () => {
    set({ isLoading: true })
    try {
      const res = await friendshipApi.getFriends()
      if (res.isSuccess && res.data) {
        set({ friends: res.data })
      }
    } finally {
      set({ isLoading: false })
    }
  },

  fetchPendingRequests: async () => {
    try {
      const res = await friendshipApi.getPendingRequests()
      if (res.isSuccess && res.data) {
        set({ pendingRequests: res.data })
      }
    } catch (error) {
      console.error('Failed to fetch requests', error)
    }
  },

  acceptRequest: async (friendshipId) => {
    const request = get().pendingRequests.find((r) => r.friendshipId === friendshipId)

    try {
      await friendshipApi.acceptRequest(friendshipId)
      set((state) => ({
        pendingRequests: state.pendingRequests.filter((req) => req.friendshipId !== friendshipId)
      }))

      get().fetchFriends()

      if (request?.userId) {
        await chatApi.getOrCreateDirectChat(request.userId)

        const currentUser = useAuthStore.getState().user
        if (currentUser?.id) {
          useChatStore.getState().fetchChats(currentUser.id)
        }
      }
    } catch (error) {
      console.error('Error accepting request:', error)
    }
  },

  declineRequest: async (friendshipId) => {
    try {
      await friendshipApi.declineRequest(friendshipId)
      set((state) => ({
        pendingRequests: state.pendingRequests.filter((req) => req.friendshipId !== friendshipId)
      }))
    } catch (error) {
      console.error(error)
    }
  },

  clearSearch: () => set({ searchResults: [] })
}))
