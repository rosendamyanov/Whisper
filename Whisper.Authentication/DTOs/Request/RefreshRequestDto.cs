namespace Whisper.Authentication.DTOs.Request
{
    public class RefreshRequestDTO
    {
        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }
        public required Guid RefreshTokenId { get; set; }
    }
}
