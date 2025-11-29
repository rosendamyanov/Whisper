using Whisper.Data.Models;
using Whisper.DTOs.Request.LiveStream;
using Whisper.DTOs.Response.LiveStream;
using Whisper.Services.Factories.Interfaces;

namespace Whisper.Services.Factories
{
    public class LiveStreamFactory : ILiveStreamFactory
    {
        public LiveStream Create(Guid userId, Guid chatId)
        {
            return new LiveStream
            {
                Id = Guid.NewGuid(),
                HostUserId = userId,
                ChatId = chatId,
                StartedAt = DateTime.UtcNow,
            };
        }

        public LiveStreamResponseDto ToDto(LiveStream stream)
        {
            return new LiveStreamResponseDto
            {
                Id = stream.Id,
                ChatId = stream.ChatId,
                Host = new LiveStreamHostResponsetDto
                {
                    Id = stream.HostUser.Id,
                    Username = stream.HostUser.Username
                },
                StartedAt = stream.StartedAt,
                IsLive = stream.IsLive
            };
        }
    }
}
