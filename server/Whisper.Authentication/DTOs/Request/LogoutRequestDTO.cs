namespace Whisper.Authentication.DTOs.Request
{
    public class LogoutRequestDTO
    {
        public string? RefreshToken { get; set; }
        public Guid? RefreshTokenId { get; set; }
    }
}
