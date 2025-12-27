// Match the C# MessageResponseDto structure
export interface MessageResponse {
    id: string;
    content: string | null;
    sentAt: string;
    isEdited: boolean;
    editedAt: string | null;
    isPinned: boolean;
    type: string;

    // Sender Info
    senderId: string;
    senderName: string;
    senderAvatarUrl: string | null;
    isMe: boolean;

    // Reply Logic
    replyToId: string | null;
    replyToSenderName: string | null;
    replyToContent: string | null;

    // Collections (You can define specific types for these later if needed)
    attachments: any[]; 
    reactions: any[]; 
    readBy: any[];
}