namespace Whisper.DTOs.Request.Chat
{
    public class AddUserToChatRequestDTO
    {
        public Guid ChatId { get; set; }
        public Guid UserId { get; set; }
    }
}
