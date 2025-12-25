using Whisper.DTOs.Response.Message;

namespace Whisper.Services.Services.Interfaces
{
    public interface IChatNotificationService
    {
        Task BroadcastMessageAsync(Guid chatId, MessageResponseDto message);
        Task BroadcastMessageEditedAsync(Guid chatId, MessageResponseDto message);
        Task BroadcastMessageDeletedAsync(Guid chatId, Guid messageId);
        Task BroadcastReactionChangedAsync(Guid chatId, Guid messageId, List<ReactionResponseDto> reactions);
        Task BroadcastMessagesReadAsync(Guid chatId, Guid userId, List<Guid> messageIds);
    }
}
