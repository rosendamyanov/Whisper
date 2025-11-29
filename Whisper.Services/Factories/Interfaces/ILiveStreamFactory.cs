using Whisper.Data.Models;
using Whisper.DTOs.Request.LiveStream;

namespace Whisper.Services.Factories.Interfaces
{
    public interface ILiveStreamFactory
    {
        LiveStream Create(Guid userId, Guid chatId);
        LiveStreamResponseDto ToDto(LiveStream stream);
    }
}
