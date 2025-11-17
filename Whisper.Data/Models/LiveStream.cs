namespace Whisper.Data.Models
{
    public class LiveStream
    {
        public Guid Id { get; set; }

        public Guid HostUserId { get; set; }
        public User HostUser { get; set; }

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public bool IsLive { get; set; } = true;
    }
}
