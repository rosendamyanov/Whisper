namespace Whisper.Data.Models
{
    public class Chat
    {
        public Guid Id { get; set; }
        public bool IsGroup { get; set; }
        public string? Name { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public LiveStream? ActiveStream { get; set; }
        public Guid? ActiveStreamId { get; set; }

        public ICollection<User> Users { get; set; } = new List<User>();
        public ICollection<Message> Messages { get; set; } = new List<Message>();
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
    }
}
