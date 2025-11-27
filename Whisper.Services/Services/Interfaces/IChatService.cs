using Whisper.Common.Response;
using Whisper.DTOs.Request.Chat;
using Whisper.DTOs.Response.Chat;

namespace Whisper.Services.Services.Interfaces
{
    public interface IChatService
    {
        Task<ApiResponse<ChatResponseDTO>> CreateGroupChatAsync(Guid currentUser, CreateGroupChatRequestDTO request);
        Task<ApiResponse<ChatResponseDTO>> GetOrCreateDirectChatAsync(Guid currentUserId, Guid friendId);
        Task<ApiResponse<MessageResponseDTO>> SendMessageAsync(Guid userId, SendMessageRequestDTO request);
        Task<ApiResponse<List<MessageResponseDTO>>> GetChatMessagesAsync(Guid chatId, Guid userId, int limit = 50, DateTime? before = null);
        Task<ApiResponse<List<ChatResponseDTO>>> GetUserChatsAsync(Guid userId);
    }
}