namespace Whisper.Data.Models.Messages
{
    public class MessageReaction
    {
        public Guid Id { get; set; }

        public string Content { get; set; } // The emoji (e.g., "❤️")
        public DateTime ReactedAt { get; set; } = DateTime.UtcNow;

        // Relationships
        public Guid MessageId { get; set; }
        public Message Message { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; }
    }
}