import { User } from '../api/response/user/User'

export type UserStatus = 'online' | 'away' | 'dnd' | 'offline'

export interface ChatMember extends User {
  status?: UserStatus
}

export interface UiChatPreview {
  id: string
  title: string
  avatarUrl: string

  status?: UserStatus

  lastMessage?: {
    content: string
    senderName: string
    time: string
    isMe: boolean
  }

  unreadCount: number
  type: 'direct' | 'group'

  isStreaming?: boolean

  members?: ChatMember[]
}
