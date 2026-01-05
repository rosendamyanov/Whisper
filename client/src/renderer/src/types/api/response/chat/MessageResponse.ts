export interface AttachmentResponse {
  url: string
  fileType: string
  fileName: string
  fileSize: number
}

export interface ReactionResponse {
  emoji: string
  count: number
  isReactedByMe: boolean
}

export interface ReadReceiptResponse {
  userId: string
  username: string
  avatarUrl: string | null
  readAt: string
}

export interface MessageResponse {
  id: string
  content: string | null
  sentAt: string
  isEdited: boolean
  editedAt: string | null
  isPinned: boolean
  type: string

  senderId: string
  senderName: string
  senderAvatarUrl: string | null
  isMe: boolean

  replyToId: string | null
  replyToSenderName: string | null
  replyToContent: string | null

  attachments: AttachmentResponse[]
  reactions: ReactionResponse[]
  readBy: ReadReceiptResponse[]
}

export interface ChatLoadResponse {
  messages: MessageResponse[]
  unreadCount: number
}
