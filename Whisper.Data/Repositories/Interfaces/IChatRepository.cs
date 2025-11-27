using Whisper.Data.Models;

namespace Whisper.Data.Repositories.Interfaces
{
    public interface IChatRepository
    {
        Task<Message> CreateMessageAsync(Message message);
        Task<bool> AddUserToChatAsync(Guid chatId, Guid userId);
        Task<Chat> CreateChatAsync(Chat chat);
        Task<Chat?> GetChatByIdAsync(Guid chatId);
        Task<List<Message>> GetChatMessagesAsync(Guid chatId, int limit, DateTime? before);
        Task<Chat?> GetDirectMessageChatAsync(Guid userId1, Guid userId2);
        Task<User?> GetUserByIdAsync(Guid userId);
        Task<List<Chat>> GetUserChatsAsync(Guid userId);
        Task<List<User>> GetUsersByIdsAsync(List<Guid> userIds);
        Task<bool> IsUserInChatAsync(Guid chatId, Guid userId);
        Task<bool> RemoveUserFromChatAsync(Guid chatId, Guid userId);
        Task<bool> UpdateChatAsync(Chat chat);
    }
}