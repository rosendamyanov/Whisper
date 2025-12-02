namespace Whisper.Data.Models
{
    public class VoiceSession
    {
        public Guid Id { get; set; }

        public Guid HostUserId { get; set; }
        public User HostUser { get; set; }

        public Guid ChatId { get; set; }
        public Chat Chat { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime StartedAt { get; set; } = DateTime.UtcNow;

        public ICollection<VoiceParticipant> Participants { get; set; } = new List<VoiceParticipant>();
    }
}
