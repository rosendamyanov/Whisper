namespace Whisper.Data.Models.Authentication
{
    public class RevokedToken
    {
        public Guid Id { get; set; }
        public string TokenHash { get; set; }
        public DateTime RevokedAt { get; set; } = DateTime.UtcNow;
        public string Reason { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } 
    }
}
