namespace Whisper.Data.Models
{
    public class Friendship
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; }

        public Guid FriendId { get; set; }
        public User Friend { get; set; }

        public bool IsAccepted { get; set; } = false;

        public bool IsBlocked { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? AcceptedAt { get; set; }
    }
}
