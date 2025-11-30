using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Whisper.Services.Services.Interfaces;

namespace Whisper.Services.Hubs
{
    [Authorize]
    public class StreamHub : Hub
    {
        private readonly ILiveStreamService _liveStreamService;

        // Track viewer counts per stream (in-memory)
        private static readonly Dictionary<Guid, HashSet<string>> _streamViewers = new();
        private static readonly object _lock = new();

        public StreamHub(ILiveStreamService liveStreamService)
        {
            _liveStreamService = liveStreamService;
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

        // ====== Stream Lifecycle ======

        public async Task NotifyStreamStarted(Guid chatId, Guid streamId)
        {
            await Clients.Group($"chat_{chatId}").SendAsync("StreamStarted", new
            {
                StreamId = streamId,
                HostId = GetUserId(),
                HostUsername = GetUsername()
            });
        }

        public async Task NotifyStreamEnded(Guid chatId, Guid streamId)
        {
            await Clients.Group($"chat_{chatId}").SendAsync("StreamEnded", new
            {
                StreamId = streamId
            });

            // Clean up viewers
            lock (_lock)
            {
                _streamViewers.Remove(streamId);
            }
        }

        // ====== Viewer Management ======

        public async Task JoinStream(Guid streamId)
        {
            var userId = GetUserId();
            var username = GetUsername();

            await Groups.AddToGroupAsync(Context.ConnectionId, $"stream_{streamId}");

            lock (_lock)
            {
                if (!_streamViewers.ContainsKey(streamId))
                    _streamViewers[streamId] = new HashSet<string>();

                _streamViewers[streamId].Add(userId.ToString());
            }

            await Clients.Group($"stream_{streamId}").SendAsync("ViewerJoined", new
            {
                UserId = userId,
                Username = username,
                ViewerCount = GetViewerCount(streamId)
            });
        }

        public async Task LeaveStream(Guid streamId)
        {
            var userId = GetUserId();
            var username = GetUsername();

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"stream_{streamId}");

            lock (_lock)
            {
                if (_streamViewers.ContainsKey(streamId))
                    _streamViewers[streamId].Remove(userId.ToString());
            }

            await Clients.Group($"stream_{streamId}").SendAsync("ViewerLeft", new
            {
                UserId = userId,
                Username = username,
                ViewerCount = GetViewerCount(streamId)
            });
        }

        public int GetViewerCount(Guid streamId)
        {
            lock (_lock)
            {
                return _streamViewers.ContainsKey(streamId)
                    ? _streamViewers[streamId].Count
                    : 0;
            }
        }

        // ====== WebRTC Signaling ======

        public async Task SendOffer(Guid streamId, string targetUserId, string offer)
        {
            await Clients.User(targetUserId).SendAsync("ReceiveOffer", new
            {
                StreamId = streamId,
                FromUserId = GetUserId(),
                Offer = offer
            });
        }

        public async Task SendAnswer(Guid streamId, string targetUserId, string answer)
        {
            await Clients.User(targetUserId).SendAsync("ReceiveAnswer", new
            {
                StreamId = streamId,
                FromUserId = GetUserId(),
                Answer = answer
            });
        }

        public async Task SendIceCandidate(Guid streamId, string targetUserId, string candidate)
        {
            await Clients.User(targetUserId).SendAsync("ReceiveIceCandidate", new
            {
                StreamId = streamId,
                FromUserId = GetUserId(),
                Candidate = candidate
            });
        }

        // ====== Connection Handling ======

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetUserId().ToString();

            // Remove user from all streams they were watching
            lock (_lock)
            {
                foreach (var stream in _streamViewers)
                {
                    if (stream.Value.Remove(userId))
                    {
                        // Notify remaining viewers
                        Clients.Group($"stream_{stream.Key}").SendAsync("ViewerLeft", new
                        {
                            UserId = userId,
                            ViewerCount = stream.Value.Count
                        });
                    }
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        // ====== Chat Group Management ======

        public async Task JoinChatGroup(Guid chatId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"chat_{chatId}");
        }

        public async Task LeaveChatGroup(Guid chatId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"chat_{chatId}");
        }
    }
}