using System.Collections.Concurrent;

namespace Whisper.DTOs.SignalR
{
    public class VoiceSessionState
    {
        public Guid Id { get; set; }
        public Guid ChatId { get; set; }
        public DateTime StartedAt { get; set; }
        public ConcurrentDictionary<Guid, VoiceParticipantDto> Participants { get; set; } = new();

        public bool IsEmpty => Participants.IsEmpty;
        public int ParticipantCount => Participants.Count;
    }
}
