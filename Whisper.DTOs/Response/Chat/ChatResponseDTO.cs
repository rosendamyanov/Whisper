using Whisper.Data.Models;

namespace Whisper.DTOs.Response.Chat
{
    public class ChatResponseDTO
    {
        public Guid Id { get; set; }
        public bool IsGroup { get; set; }
        public string? Name { get; set; }
        public List<UserBasicDTO> Participants { get; set; }
        public DateTime CreatedAt { get; set; }
        public MessageResponseDTO LastMessage { get; set; }
    }
}
