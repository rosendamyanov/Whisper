namespace Whisper.DTOs.Response.Chat
{
    public class MessageResponseDTO
    {
        public Guid Id { get; set; }
        public string Content { get; set; }
        public DateTime SentAt { get; set; }
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public Guid ChatId { get; set; }
    }
}
