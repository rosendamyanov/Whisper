using System.ComponentModel.DataAnnotations;

namespace Whisper.Data.Models
{
    public class Chat
    {
        [Key]
        public Guid Id { get; set; }
        public bool IsGroup { get; set; }

        public string? Name { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
