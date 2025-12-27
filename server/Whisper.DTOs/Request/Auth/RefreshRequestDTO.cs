namespace Whisper.DTOs.Request.Auth
{
    public class RefreshRequestDTO
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public Guid? RefreshTokenId { get; set; }
    }
}
