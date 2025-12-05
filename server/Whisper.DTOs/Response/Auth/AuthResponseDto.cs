namespace Whisper.DTOs.Response.Auth
{
    public class AuthResponseDto
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public Guid RefreshTokenId { get; set; }
        public DateTime AccessTokenExpiry { get; set; }
    }
}
