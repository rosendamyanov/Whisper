namespace Whisper.DTOs.Request.Chat
{
    public class SendMessageRequestDTO
    {
        public Guid ChatId { get; set; }
        public string Content { get; set; }
    }
}
