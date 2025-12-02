using Whisper.Data.Models;

namespace Whisper.Data.Repositories.Interfaces
{
    public interface ILiveStreamRepository
    {
        Task<bool> EndStreamAsync(Guid streamId, Guid userId);
        Task<LiveStream?> GetActiveStreamByChatIdAsync(Guid chatId);
        Task<LiveStream?> GetByIdAsync(Guid streamId);
        Task<bool> IsUserStreamingAsync(Guid userId);
        Task<LiveStream> StartStreamAsync(LiveStream stream);
    }
}