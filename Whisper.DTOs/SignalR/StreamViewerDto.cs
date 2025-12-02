namespace Whisper.DTOs.SignalR
{
    public class StreamViewerDto
    {
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public string ConnectionId { get; set; }
        public DateTime JoinedAt { get; set; }
    }
}