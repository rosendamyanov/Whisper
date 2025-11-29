namespace Whisper.Data.Models
{
    public class LiveStream
    {
        public Guid Id { get; set; }

        public Guid HostUserId { get; set; }
        public User HostUser { get; set; }

        public Guid ChatId { get; set; }
        public Chat Chat { get; set; }

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? EndedAt { get; set; }

        public bool IsLive { get; set; } = true;
    }
}
