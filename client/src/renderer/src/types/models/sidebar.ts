export type UserStatus = 'online' | 'away' | 'dnd' | 'offline';

export interface UiChatPreview {
    id: string;
    title: string;
    avatarUrl: string;

    status?: UserStatus;

    lastMessage?: {
        content: string;
        senderName: string;
        time: string;
        isMe: boolean;
    };

    unreadCount: number;
    type: 'direct' | 'group';

    isStreaming?: boolean;
}