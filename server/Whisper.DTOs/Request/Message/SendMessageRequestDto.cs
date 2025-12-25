using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Whisper.DTOs.Request.Message
{
    public class SendMessageRequestDto
    {
        [Required]
        public Guid ChatId { get; set; }

        public string? Content { get; set; }

        public Guid? ReplyToId { get; set; }

        public List<Guid> MentionedUserIds { get; set; } = new();

        public List<IFormFile>? Files { get; set; }
    }
}
