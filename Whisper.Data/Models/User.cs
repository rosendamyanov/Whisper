using Whisper.Data.Models.Authentication;

namespace Whisper.Data.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Role { get; set; } = "User";
        public UserCredentials Credentials { get; set; }
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public ICollection<RevokedToken> RevokedTokens { get; set; } = new List<RevokedToken>();

        public ICollection<Chat> Chats { get; set; } = new List<Chat>();
        public ICollection<Message> MessagesSent { get; set; } = new List<Message>();
    }
}
