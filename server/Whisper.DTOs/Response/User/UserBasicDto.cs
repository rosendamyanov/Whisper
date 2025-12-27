namespace Whisper.DTOs.Response.User
{
    public class UserBasicDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
        public string ProfilePictureUrl { get; set; }
    }
}
