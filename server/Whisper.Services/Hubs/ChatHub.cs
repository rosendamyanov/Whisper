using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Whisper.Data.Repositories.Interfaces;
using Whisper.Services.Services.Interfaces;

namespace Whisper.Services.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IPresenceTracker _presenceTracker;
        private readonly IFriendshipRepository _friendshipRepository;

        public ChatHub(IPresenceTracker presenceTracker, IFriendshipRepository friendshipRepository)
        {
            _presenceTracker = presenceTracker;
            _friendshipRepository = friendshipRepository;
        }

        private Guid GetUserId()
        {
            var userIdClaim = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(userIdClaim, out var id) ? id : Guid.Empty;
        }

        public async Task JoinChat(Guid chatId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, chatId.ToString());  
        }

        public async Task LeaveChat(Guid chatId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId.ToString());
        }

        public async Task StartTyping(Guid chatId)
        {
            await Clients.OthersInGroup(chatId.ToString()).SendAsync("UserTyping", new
            {
                UserId = GetUserId(),
                Username = Context.User?.Identity?.Name ?? "Unknown"
            });
        }

        public async Task StopTyping(Guid chatId)
        {
            await Clients.OthersInGroup(chatId.ToString()).SendAsync("UserStoppedTyping", new
            {
                UserId = GetUserId()
            });
        }

        public override async Task OnConnectedAsync()
        {
            var userId = GetUserId();
            var isFirstConnection = await _presenceTracker.UserConnected(userId, Context.ConnectionId);

            if (isFirstConnection)
            {
                var friendIds = await _friendshipRepository.GetFriendsIdsAsync(userId);
                var friendStrings = friendIds.Select(id => id.ToString()).ToList();

                if (friendStrings.Any())
                {
                    await Clients.Users(friendStrings).SendAsync("UserOnline", userId);
                }
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetUserId();
            var isFullyOffline = await _presenceTracker.UserDisconnected(userId, Context.ConnectionId);

            if (isFullyOffline)
            {
                var friendIds = await _friendshipRepository.GetFriendsIdsAsync(userId);
                var friendStrings = friendIds.Select(id => id.ToString()).ToList();

                if (friendStrings.Any())
                {
                    await Clients.Users(friendStrings).SendAsync("UserOffline", userId);
                }

                // TODO (Future): Update 'LastActive' in User Table here
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}