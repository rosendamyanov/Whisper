using Whisper.Common.Response;
using Whisper.DTOs.Request.LiveStream;

namespace Whisper.Services.Services.Interfaces
{
    public interface ILiveStreamService
    {
        Task<ApiResponse<LiveStreamResponseDto>> GetActiveStreamAsync(Guid chatId, Guid userId);
        Task<ApiResponse<LiveStreamResponseDto>> StartStreamAsync(Guid chatId, Guid userId);
        Task<ApiResponse<string>> EndStreamAsync(Guid streamId, Guid userId);
    }
}
