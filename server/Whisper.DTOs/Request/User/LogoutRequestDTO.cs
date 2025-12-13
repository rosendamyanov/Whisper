namespace Whisper.DTOs.Request.User
{
    public class LogoutRequestDTO
    {
        public string? RefreshToken { get; set; }
        public Guid? RefreshTokenId { get; set; }
    }
}
