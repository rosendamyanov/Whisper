using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using Whisper.Data.Repositories.Interfaces;
using Whisper.DTOs.SignalR;

namespace Whisper.Services.Hubs
{
    [Authorize]
    public class StreamHub : Hub
    {
        private readonly IChatRepository _chatRepository;
        private static readonly ConcurrentDictionary<Guid, StreamSessionState> _activeStreams = new();
        private static readonly ConcurrentDictionary<Guid, (Guid ChatId, string ConnectionId)> _viewerSessions = new();
        private static readonly ConcurrentDictionary<Guid, Guid> _hostSessions = new();

        public StreamHub(IChatRepository chatRepository)
        {
            _chatRepository = chatRepository;
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

        // ====== Stream Lifecycle (Host) ======

        /// <summary>
        /// Starts a new stream in a chat. Only one stream per chat allowed.
        /// User becomes the host. Only host can stream video.
        /// </summary>
        public async Task<StreamSessionState?> StartStream(Guid chatId)
        {
            var userId = GetUserId();
            var username = GetUsername();

            
            var isInChat = await _chatRepository.IsUserInChatAsync(chatId, userId);
            if (!isInChat)
            {
                await Clients.Caller.SendAsync("Error", "You are not a member of this chat.");
                return null;
            }

            
            if (_hostSessions.ContainsKey(userId))
            {
                await Clients.Caller.SendAsync("Error", "You are already hosting a stream. End it first.");
                return null;
            }

            
            if (_activeStreams.ContainsKey(chatId))
            {
                await Clients.Caller.SendAsync("Error", "This chat already has an active stream.");
                return null;
            }

            
            var stream = new StreamSessionState
            {
                Id = Guid.NewGuid(),
                ChatId = chatId,
                HostUserId = userId,
                HostUsername = username,
                StartedAt = DateTime.UtcNow
            };

            if (!_activeStreams.TryAdd(chatId, stream))
            {
                await Clients.Caller.SendAsync("Error", "Failed to start stream. Please try again.");
                return null;
            }

            _hostSessions.TryAdd(userId, chatId);

            
            await Groups.AddToGroupAsync(Context.ConnectionId, $"stream_{chatId}");

            
            await Clients.Group($"chat_{chatId}").SendAsync("StreamStarted", new
            {
                ChatId = chatId,
                StreamId = stream.Id,
                HostUserId = userId,
                HostUsername = username
            });

            return stream;
        }

        /// <summary>
        /// Ends the stream. Only the host can end their stream.
        /// All viewers are notified and disconnected from stream group.
        /// </summary>
        public async Task EndStream(Guid chatId)
        {
            var userId = GetUserId();

            if (!_activeStreams.TryGetValue(chatId, out var stream))
            {
                return;
            }

            
            if (stream.HostUserId != userId)
            {
                await Clients.Caller.SendAsync("Error", "Only the host can end the stream.");
                return;
            }

            await DestroyStream(chatId, stream);
        }

        /// <summary>
        /// Gets the current stream state for a chat if one exists.
        /// </summary>
        public Task<StreamSessionState?> GetStream(Guid chatId)
        {
            _activeStreams.TryGetValue(chatId, out var stream);
            return Task.FromResult(stream);
        }

        /// <summary>
        /// Gets all active streams for a list of chat IDs.
        /// Useful for showing stream indicators in chat list.
        /// </summary>
        public Task<Dictionary<Guid, StreamInfoDto>> GetActiveStreamsForChats(List<Guid> chatIds)
        {
            var result = new Dictionary<Guid, StreamInfoDto>();

            foreach (var chatId in chatIds)
            {
                if (_activeStreams.TryGetValue(chatId, out var stream))
                {
                    result[chatId] = new StreamInfoDto
                    {
                        StreamId = stream.Id,
                        HostUsername = stream.HostUsername,
                        ViewerCount = stream.ViewerCount
                    };
                }
            }

            return Task.FromResult(result);
        }

        // ====== Viewer Management ======

        /// <summary>
        /// Join a stream as a viewer. Cannot join your own stream as viewer.
        /// </summary>
        public async Task<StreamSessionState?> JoinStream(Guid chatId)
        {
            var userId = GetUserId();
            var username = GetUsername();
            
            var isInChat = await _chatRepository.IsUserInChatAsync(chatId, userId);
            if (!isInChat)
            {
                await Clients.Caller.SendAsync("Error", "You are not a member of this chat.");
                return null;
            }

            if (!_activeStreams.TryGetValue(chatId, out var stream))
            {
                await Clients.Caller.SendAsync("Error", "No active stream in this chat.");
                return null;
            }

            if (stream.HostUserId == userId)
            {
                await Clients.Caller.SendAsync("Error", "You are the host of this stream.");
                return null;
            }

            if (_viewerSessions.TryGetValue(userId, out var existingSession))
            {
                if (existingSession.ChatId != chatId)
                {
                    await Clients.Caller.SendAsync("Error", "You are already viewing another stream. Leave it first.");
                    return null;
                }

                return stream;
            }

            var viewer = new StreamViewerDto
            {
                UserId = userId,
                Username = username,
                ConnectionId = Context.ConnectionId,
                JoinedAt = DateTime.UtcNow
            };

            stream.Viewers.TryAdd(userId, viewer);
            _viewerSessions.TryAdd(userId, (chatId, Context.ConnectionId));

            await Groups.AddToGroupAsync(Context.ConnectionId, $"stream_{chatId}");

            await Clients.OthersInGroup($"stream_{chatId}").SendAsync("ViewerJoined", new
            {
                UserId = userId,
                Username = username,
                ViewerCount = stream.ViewerCount
            });

            await Clients.Group($"chat_{chatId}").SendAsync("StreamUpdated", new
            {
                ChatId = chatId,
                StreamId = stream.Id,
                ViewerCount = stream.ViewerCount
            });

            return stream;
        }

        /// <summary>
        /// Leave a stream as a viewer.
        /// </summary>
        public async Task LeaveStream(Guid chatId)
        {
            var userId = GetUserId();

            if (!_activeStreams.TryGetValue(chatId, out var stream))
            {
                return;
            }

            await RemoveViewerFromStream(chatId, userId, stream);
        }

        // ====== WebRTC Signaling ======

        /// <summary>
        /// Host sends offer to a specific viewer.
        /// Stream is one-to-many: host sends to each viewer individually.
        /// </summary>
        public async Task SendOffer(Guid chatId, Guid targetUserId, string offer)
        {
            if (!_activeStreams.TryGetValue(chatId, out var stream))
            {
                return;
            }

            if (stream.HostUserId != GetUserId())
            {
                return;
            }

            if (stream.Viewers.TryGetValue(targetUserId, out var viewer))
            {
                await Clients.Client(viewer.ConnectionId).SendAsync("ReceiveOffer", new
                {
                    FromUserId = stream.HostUserId,
                    FromUsername = stream.HostUsername,
                    Offer = offer
                });
            }
        }

        /// <summary>
        /// Viewer sends answer back to host.
        /// </summary>
        public async Task SendAnswer(Guid chatId, string answer)
        {
            var userId = GetUserId();

            if (!_activeStreams.TryGetValue(chatId, out var stream))
            {
                return;
            }

            if (!stream.Viewers.ContainsKey(userId))
            {
                return;
            }

            await Clients.User(stream.HostUserId.ToString()).SendAsync("ReceiveAnswer", new
            {
                FromUserId = userId,
                Answer = answer
            });
        }

        /// <summary>
        /// Send ICE candidate to establish connection.
        /// Can be called by both host (to viewer) and viewer (to host).
        /// </summary>
        public async Task SendIceCandidate(Guid chatId, Guid targetUserId, string candidate)
        {
            var userId = GetUserId();

            if (!_activeStreams.TryGetValue(chatId, out var stream))
            {
                return;
            }

            string? targetConnectionId = null;

            if (targetUserId == stream.HostUserId)
            {
                await Clients.User(stream.HostUserId.ToString()).SendAsync("ReceiveIceCandidate", new
                {
                    FromUserId = userId,
                    Candidate = candidate
                });
                return;
            }

            if (stream.Viewers.TryGetValue(targetUserId, out var viewer))
            {
                targetConnectionId = viewer.ConnectionId;
            }

            if (targetConnectionId != null)
            {
                await Clients.Client(targetConnectionId).SendAsync("ReceiveIceCandidate", new
                {
                    FromUserId = userId,
                    Candidate = candidate
                });
            }
        }

        // ====== Connection Lifecycle ======

        /// <summary>
        /// Handles disconnection. If host disconnects, stream ends.
        /// If viewer disconnects, they're removed from viewers.
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetUserId();

            if (_hostSessions.TryGetValue(userId, out var hostedChatId))
            {
                if (_activeStreams.TryGetValue(hostedChatId, out var stream))
                {
                    await DestroyStream(hostedChatId, stream);
                }
            }

            if (_viewerSessions.TryGetValue(userId, out var viewerSession))
            {
                if (_activeStreams.TryGetValue(viewerSession.ChatId, out var stream))
                {
                    await RemoveViewerFromStream(viewerSession.ChatId, userId, stream);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        // ====== Private Helpers ======

        private async Task RemoveViewerFromStream(Guid chatId, Guid userId, StreamSessionState stream)
        {
            if (stream.Viewers.TryRemove(userId, out var removedViewer))
            {
                _viewerSessions.TryRemove(userId, out _);

                await Groups.RemoveFromGroupAsync(removedViewer.ConnectionId, $"stream_{chatId}");

                await Clients.Group($"stream_{chatId}").SendAsync("ViewerLeft", new
                {
                    UserId = userId,
                    Username = removedViewer.Username,
                    ViewerCount = stream.ViewerCount
                });

                await Clients.Group($"chat_{chatId}").SendAsync("StreamUpdated", new
                {
                    ChatId = chatId,
                    StreamId = stream.Id,
                    ViewerCount = stream.ViewerCount
                });
            }
        }

        private async Task DestroyStream(Guid chatId, StreamSessionState stream)
        {
            foreach (var viewer in stream.Viewers.Values.ToList())
            {
                _viewerSessions.TryRemove(viewer.UserId, out _);
                await Groups.RemoveFromGroupAsync(viewer.ConnectionId, $"stream_{chatId}");
            }

            _hostSessions.TryRemove(stream.HostUserId, out _);
            
            _activeStreams.TryRemove(chatId, out _);

            await Clients.Group($"stream_{chatId}").SendAsync("StreamEnded", new
            {
                ChatId = chatId,
                StreamId = stream.Id
            });

            await Clients.Group($"chat_{chatId}").SendAsync("StreamEnded", new
            {
                ChatId = chatId,
                StreamId = stream.Id
            });
        }
    }

    // Small DTO for bulk stream info query
    public class StreamInfoDto
    {
        public Guid StreamId { get; set; }
        public string HostUsername { get; set; }
        public int ViewerCount { get; set; }
    }
}