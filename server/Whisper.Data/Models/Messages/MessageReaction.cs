namespace Whisper.Data.Models.Messages
{
    public class MessageReaction
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public DateTime ReactedAt { get; set; } = DateTime.UtcNow;
        public Guid MessageId { get; set; }
        public Message Message { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
    }
}