namespace Whisper.DTOs.Request.User
{
    public class UserLoginRequestDTO
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
    }
}
