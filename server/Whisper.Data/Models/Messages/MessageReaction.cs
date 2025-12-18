using Whisper.Data.Models.Messages;

namespace Whisper.Data.Models.Messages
{
    public class MessageReceipt
    {
        public Guid Id { get; set; }

        public DateTime ReadAt { get; set; } = DateTime.UtcNow;

        // Relationships
        public Guid MessageId { get; set; }
        public Message Message { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; }
    }
}