using Microsoft.AspNetCore.SignalR;
using Whisper.DTOs.Response.Message;
using Whisper.Services.Hubs;
using Whisper.Services.Services.Interfaces;

namespace Whisper.Services.Services
{
    public class ChatNotificationService : IChatNotificationService
    {
        private readonly IHubContext<ChatHub> _chatHubContext;

        public ChatNotificationService(IHubContext<ChatHub> chatHubContext)
        {
            _chatHubContext = chatHubContext;
        }   

        public async Task BroadcastMessageAsync(Guid chatId, MessageResponseDto message)
        {
            await _chatHubContext.Clients.Group(chatId.ToString())
                .SendAsync("ReceiveMessage", message);
        }

        public async Task BroadcastMessageEditedAsync(Guid chatId, MessageResponseDto message)
        {
            await _chatHubContext.Clients.Group(chatId.ToString())
                .SendAsync("MessageEdited", message);
        }

        public async Task BroadcastMessageDeletedAsync(Guid chatId, Guid messageId)
        {
            await _chatHubContext.Clients.Group(chatId.ToString())
                .SendAsync("MessageDeleted", messageId);
        }

        public async Task BroadcastReactionChangedAsync(Guid chatId, Guid messageId, List<ReactionResponseDto> reactions)
        {
            await _chatHubContext.Clients.Group(chatId.ToString())
                .SendAsync("ReactionsUpdated", new
                {
                    MessageId = messageId,
                    Reactions = reactions
                });
        }

        public async Task BroadcastMessagesReadAsync(Guid chatId, Guid userId, List<Guid> messageIds)
        {
            await _chatHubContext.Clients.Group(chatId.ToString())
                .SendAsync("MessagesRead", new
                {
                    UserId = userId,
                    MessageIds = messageIds
                });
        }
    }
}
