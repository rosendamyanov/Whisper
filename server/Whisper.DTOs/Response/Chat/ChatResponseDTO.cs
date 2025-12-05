using Whisper.Data.Models;
using Whisper.DTOs.Response.User;

namespace Whisper.DTOs.Response.Chat
{
    public class ChatResponseDTO
    {
        public Guid Id { get; set; }
        public bool IsGroup { get; set; }
        public string? Name { get; set; }
        public List<UserBasicDto> Participants { get; set; }
        public DateTime CreatedAt { get; set; }
        public MessageResponseDTO LastMessage { get; set; }
    }
}
