namespace Whisper.DTOs.Response.Message
{
    public class ChatLoadResponseDto
    {
        public List<MessageResponseDto> Messages { get; set; } = new();
        public int UnreadCount { get; set; }
    }
}
