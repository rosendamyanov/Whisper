using Whisper.Common.Response;
using Whisper.DTOs.Request.Message;
using Whisper.DTOs.Response.Message;

namespace Whisper.Services.Services.Interfaces
{
    public interface IMessageService
    {
        Task<ApiResponse<MessageResponseDto>> SendMessageAsync(Guid userId, SendMessageRequestDto request);

        Task<ApiResponse<MessageResponseDto>> EditMessageAsync(Guid userId, Guid messageId, string newContent);

        Task<ApiResponse<ChatLoadResponseDto>> GetChatMessagesAsync(Guid userId, Guid chatId, int limit, DateTime? before);

        Task<ApiResponse<bool>> ReadMessagesAsync(Guid userId, List<Guid> messageIds);

        Task<ApiResponse<bool>> ReactToMessageAsync(Guid userId, Guid messageId, string emoji);
        Task<ApiResponse<bool>> DeleteMessageAsync(Guid userId, Guid messageId);
    }
}
