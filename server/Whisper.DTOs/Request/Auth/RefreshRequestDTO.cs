namespace Whisper.DTOs.Request.Auth
{
    public class RefreshRequestDTO
    {
        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }
        public required Guid RefreshTokenId { get; set; }
    }
}
