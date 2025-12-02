using System.Collections.Concurrent;

namespace Whisper.DTOs.SignalR
{
    public class StreamSessionState
    {
        public Guid Id { get; set; }
        public Guid ChatId { get; set; }
        public Guid HostUserId { get; set; }
        public string HostUsername { get; set; }
        public DateTime StartedAt { get; set; }
        public ConcurrentDictionary<Guid, StreamViewerDto> Viewers { get; set; } = new();

        public bool HasViewers => !Viewers.IsEmpty;
        public int ViewerCount => Viewers.Count;
    }
}