using Whisper.DTOs.Response.LiveStream;

namespace Whisper.DTOs.Request.LiveStream
{
    public class LiveStreamResponseDto
    {
        public Guid Id { get; set; }
        public Guid ChatId { get; set; }
        public LiveStreamHostResponsetDto Host { get; set; }
        public DateTime StartedAt { get; set; }
        public bool IsLive { get; set; }
    }
}
