namespace Whisper.Data.Models
{
    public class VoiceParticipant
    {
        public Guid Id { get; set; }

        public Guid VoiceSessionId { get; set; }
        public VoiceSession VoiceSession { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; }

        public bool IsMuted { get; set; } = false;
        public bool IsDeafened { get; set; } = false;
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}
