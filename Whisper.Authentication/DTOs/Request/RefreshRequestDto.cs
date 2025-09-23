namespace Whisper.Authentication.DTOs.Request
{
    public class RefreshRequestDto
    {
        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }
        public required Guid RefreshTokenId { get; set; }
    }
}
