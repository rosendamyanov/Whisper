namespace Whisper.Data.Models.Messages
{
    public class Message
    {
        public Guid Id { get; set; }
        public string? Content { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public DateTime? EditedAt { get; set; }
        public MessageType Type { get; set; } = MessageType.Text;
        public bool IsPinned { get; set; } = false;

        public Guid UserId { get; set; }
        public User User { get; set; }

        public Guid ChatId { get; set; }
        public Chat Chat { get; set; }

        public Guid? ReplyToId { get; set; }
        public Message? ReplyTo { get; set; }

        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        public ICollection<MessageAttachment> Attachments { get; set; } = new List<MessageAttachment>();
        public ICollection<MessageReceipt> ReadReceipts { get; set; } = new List<MessageReceipt>();
        public ICollection<MessageReaction> Reactions { get; set; } = new List<MessageReaction>();
    }
}