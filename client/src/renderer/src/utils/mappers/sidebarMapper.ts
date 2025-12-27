import { ChatResponse } from "@renderer/types/api/response/chat/ChatResponse";
import { UiChatPreview } from "@renderer/types/models/sidebar";

export const mapChatToPreview = (chat: ChatResponse, currentUserId: string): UiChatPreview => {
    // Grab last message if exists
    const lastMessage = chat.lastMessage;

    const isGroup = chat.isGroup;

    let displayTitle = 'Unknown';
    let displayAvatar = '';
    
    if (isGroup) {
        displayTitle = chat.name || 'Group Chat';
        displayAvatar = 'https://res.cloudinary.com/dpfnd2zns/image/upload/v1725459976/j1b8ezfwl0dxepguecte.jpg';
    } else {
        const otherPerson = chat.participants.find(p => p.id !== currentUserId);
        displayTitle = otherPerson?.username || 'Unknown User';
        displayAvatar = otherPerson?.avatar || 'https://res.cloudinary.com/dpfnd2zns/image/upload/v1725459976/j1b8ezfwl0dxepguecte.jpg';
    }

    return {
        id: chat.id,
        title: displayTitle,
        avatarUrl: displayAvatar,
        // If your ChatResponse doesn't have unreadCount yet, hardcode 0 for now
        unreadCount: 0, 
        type: isGroup ? 'group' : 'direct',
        status: 'offline',
        members: chat.participants || [],

        // 4. Map the fields correctly using 'senderId' and 'senderName'
        lastMessage: lastMessage ? {
            // Content might be null (e.g. image only), provide fallback
            content: lastMessage.content || 'Sent an attachment',
            
            // FIX: Use 'senderId' instead of 'userId'
            isMe: lastMessage.senderId === currentUserId,
            
            // FIX: Use 'senderId' and 'senderName' instead of 'userId' and 'username'
            senderName: lastMessage.senderId === currentUserId ? 'You' : (lastMessage.senderName || 'Unknown'),
            
            // Map sentAt -> time
            time: new Date(lastMessage.sentAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
        } : undefined, 
    };
}