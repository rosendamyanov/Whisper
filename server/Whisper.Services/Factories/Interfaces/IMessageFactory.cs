using Whisper.Data.Models.Messages;
using Whisper.DTOs.Internal;
using Whisper.DTOs.Request.Message;
using Whisper.DTOs.Response.Message;

namespace Whisper.Services.Factories.Interfaces
{
    public interface IMessageFactory
    {
        Message Map(Guid userId, SendMessageRequestDto request, List<FileSaveResult>? attachementsInfo);
        MessageResponseDto Map(Message message, Guid currentUserId);
        ChatLoadResponseDto Map(List<Message> messages, Guid currentUserId, int unreadCount);
        MessageReaction Map(Guid userId, Guid messageId, string emoji);
    }
}