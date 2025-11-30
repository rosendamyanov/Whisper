using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Whisper.DTOs.Request.Chat;
using Whisper.Services.Services.Interfaces;

namespace Whisper.Services.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;

        public ChatHub(IChatService chatService)
        {
            _chatService = chatService;
        }

        private Guid GetUserId()
        {
            var userIdClaim = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim!);
        }

        private string GetUsername()
        {
            return Context.User?.Identity?.Name ?? "Unknown";
        }

        public async Task JoinChat(Guid chatId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"chat_{chatId}");
        }

        public async Task LeaveChat(Guid chatId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"chat_{chatId}");
        }

        public async Task SendMessage(Guid chatId, string content)
        {
            var userId = GetUserId();

            // Save to database
            var result = await _chatService.SendMessageAsync(userId, new SendMessageRequestDTO
            {
                ChatId = chatId,
                Content = content
            });

            if (result.IsSuccess)
            {
                // Broadcast the saved message (with real ID and timestamp from DB)
                await Clients.Group($"chat_{chatId}").SendAsync("ReceiveMessage", result.Data);
            }
        }

        public async Task StartTyping(Guid chatId)
        {
            await Clients.OthersInGroup($"chat_{chatId}").SendAsync("UserTyping", new
            {
                UserId = GetUserId(),
                Username = GetUsername()
            });
        }

        public async Task StopTyping(Guid chatId)
        {
            await Clients.OthersInGroup($"chat_{chatId}").SendAsync("UserStoppedTyping", new
            {
                UserId = GetUserId()
            });
        }
    }
}