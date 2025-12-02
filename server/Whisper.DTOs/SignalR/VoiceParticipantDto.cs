namespace Whisper.DTOs.SignalR
{
    public class VoiceParticipantDto
    {
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public string ConnectionId { get; set; }
        public bool IsMuted { get; set; }
        public bool IsDeafened { get; set; }
        public bool IsSpeaking { get; set; }
        public DateTime JoinedAt { get; set; }
    }
}
