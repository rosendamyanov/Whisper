namespace Whisper.Data.Models
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
    }
}
