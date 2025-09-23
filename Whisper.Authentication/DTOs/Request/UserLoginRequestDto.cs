namespace Whisper.Authentication.DTOs.Request
{
    public class UserLoginRequestDTO
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
    }
}
