using Whisper.DTOs.Response.Auth;

namespace Whisper.DTOs.Response.User
{
    public class UserSessionResponseDto
    {
        public AuthResponseDto Session { get; set; } 
        public UserBasicDto User { get; set; }
    }
}
