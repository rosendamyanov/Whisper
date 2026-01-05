import { ChatResponse } from '@renderer/types/api/response/chat/ChatResponse'
import { UiChatPreview } from '@renderer/types/models/sidebar'

export const mapChatToPreview = (chat: ChatResponse, currentUserId: string): UiChatPreview => {
  const lastMessage = chat.lastMessage

  const isGroup = chat.isGroup

  let displayTitle = 'Unknown'
  let displayAvatar = ''

  if (isGroup) {
    displayTitle = chat.name || 'Group Chat'
    displayAvatar =
      'https://res.cloudinary.com/dpfnd2zns/image/upload/v1725459976/j1b8ezfwl0dxepguecte.jpg'
  } else {
    const otherPerson = chat.participants.find((p) => p.id !== currentUserId)
    displayTitle = otherPerson?.username || 'Unknown User'
    displayAvatar =
      otherPerson?.avatar ||
      'https://res.cloudinary.com/dpfnd2zns/image/upload/v1725459976/j1b8ezfwl0dxepguecte.jpg'
  }

  return {
    id: chat.id,
    title: displayTitle,
    avatarUrl: displayAvatar,
    unreadCount: 0,
    type: isGroup ? 'group' : 'direct',
    status: 'offline',
    members: chat.participants || [],

    lastMessage: lastMessage
      ? {
          content: lastMessage.content || 'Sent an attachment',

          isMe: lastMessage.senderId === currentUserId,

          senderName:
            lastMessage.senderId === currentUserId ? 'You' : lastMessage.senderName || 'Unknown',

          time: new Date(lastMessage.sentAt).toLocaleTimeString([], {
            hour: '2-digit',
            minute: '2-digit'
          })
        }
      : undefined
  }
}
