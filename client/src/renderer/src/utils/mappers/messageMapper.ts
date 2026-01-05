import { MessageResponse } from '../../types/api/response/chat/MessageResponse'

export interface UiMessage {
  id: string
  content: string | null
  sentAt: string
  displayTime: string

  senderId: string
  senderName: string
  senderAvatarUrl: string | null
  senderInitials: string

  isMe: boolean
  type: string
}

export const mapMessageToUi = (msg: MessageResponse, currentUserId?: string): UiMessage => {
  const isMe = msg.senderId === currentUserId
  const senderName = msg.senderName || 'Unknown'

  return {
    id: msg.id,
    content: msg.content,
    sentAt: msg.sentAt,
    displayTime: new Date(msg.sentAt).toLocaleTimeString([], {
      hour: '2-digit',
      minute: '2-digit'
    }),

    senderId: msg.senderId,
    senderName: senderName,
    senderAvatarUrl: msg.senderAvatarUrl,
    senderInitials: senderName.substring(0, 2).toUpperCase(),

    isMe: isMe,
    type: msg.type
  }
}
