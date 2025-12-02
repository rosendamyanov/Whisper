namespace Whisper.Data.Models.Authentication
{
    public class UserCredentials
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastPasswordChange { get; set; }
        public User User { get; set; }
    }
}
