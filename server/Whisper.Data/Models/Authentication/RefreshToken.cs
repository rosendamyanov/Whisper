namespace Whisper.Data.Models.Authentication
{
    public class RefreshToken
    {
        public Guid Id { get; set; }
        public string TokenHash { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
    }
}
