namespace Whisper.Data.Models
{
    public class Stream
    {
        public Guid Id { get; set; }

        public string? Title { get; set; }

        public bool IsLive { get; set; } = true;

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? EndedAt { get; set; }

        public Guid HostUserId { get; set; }
        public User HostUser { get; set; }

        public ICollection<User> Viewers { get; set; } = new List<User>();

        public int TotalViews { get; set; } = 0;

        public string? SessionId { get; set; }
    }
}
