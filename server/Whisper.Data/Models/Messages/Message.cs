namespace Whisper.Data.Models.Messages
{
    public class Message
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public Guid UserId { get; set; }
        public User User { get; set; }

        public Guid ChatId { get; set; }
        public Chat Chat { get; set; }

        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        // --- NEW ADDITIONS ---

        // 1. Reply Logic (Self-Referencing)
        public Guid? ReplyToId { get; set; }
        public Message? ReplyTo { get; set; }

        // 2. Read Receipts
        public ICollection<MessageReceipt> ReadReceipts { get; set; } = new List<MessageReceipt>();

        // 3. Reactions
        public ICollection<MessageReaction> Reactions { get; set; } = new List<MessageReaction>();
    }
}