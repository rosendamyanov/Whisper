namespace Whisper.DTOs.Response.Message
{
    public class MessageResponseDto
    {
        public Guid Id { get; set; }
        public string? Content { get; set; }
        public DateTime SentAt { get; set; }
        public bool IsEdited { get; set; }
        public DateTime? EditedAt { get; set; }
        public bool IsPinned { get; set; }
        public string Type { get; set; }

        // Sender Info
        public Guid SenderId { get; set; }
        public string SenderName { get; set; }
        public string? SenderAvatarUrl { get; set; }
        public bool IsMe { get; set; }

        // Reply Logic
        public Guid? ReplyToId { get; set; }
        public string? ReplyToSenderName { get; set; }
        public string? ReplyToContent { get; set; }

        // Collections
        public List<AttachmentResponseDto> Attachments { get; set; } = new();
        public List<ReactionResponseDto> Reactions { get; set; } = new();
        public List<ReadReceiptResponseDto> ReadBy { get; set; } = new();
    }
}