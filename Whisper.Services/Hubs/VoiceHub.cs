using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using Whisper.Data.Repositories.Interfaces;
using Whisper.DTOs.SignalR;

namespace Whisper.Services.Hubs
{
    [Authorize]
    public class VoiceHub : Hub
    {
        private readonly IChatRepository _chatRepository;
        private static readonly ConcurrentDictionary<Guid, VoiceSessionState> _activeSessions = new();
        private static readonly ConcurrentDictionary<Guid, (Guid ChatId, string ConnectionId)> _userSessions = new();
        private static readonly ConcurrentDictionary<Guid, CancellationTokenSource> _sessionTimeouts = new();
        private static readonly TimeSpan SessionTimeout = TimeSpan.FromMinutes(5);

        public VoiceHub(IChatRepository chatRepository)
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

        // ====== Session Management ======

        public async Task<VoiceSessionState?> JoinOrCreateSession(Guid chatId)
        {
            var userId = GetUserId();
            var username = GetUsername();

            // Verify user is member of chat
            var isInChat = await _chatRepository.IsUserInChatAsync(chatId, userId);
            if (!isInChat)
            {
                await Clients.Caller.SendAsync("Error", "You are not a member of this chat.");
                return null;
            }

            // Check if user is already in a voice session (any session)
            if (_userSessions.TryGetValue(userId, out var existingSession))
            {
                if (existingSession.ChatId != chatId)
                {
                    await Clients.Caller.SendAsync("Error", "You are already in another voice session. Leave it first.");
                    return null;
                }
                // Already in this session, return current state
                return _activeSessions.GetValueOrDefault(chatId);
            }

            // Get or create session
            var session = _activeSessions.GetOrAdd(chatId, _ => new VoiceSessionState
            {
                Id = Guid.NewGuid(),
                ChatId = chatId,
                StartedAt = DateTime.UtcNow
            });

            // Add participant
            var participant = new VoiceParticipantDto
            {
                UserId = userId,
                Username = username,
                ConnectionId = Context.ConnectionId,
                IsMuted = false,
                IsDeafened = false,
                IsSpeaking = false,
                JoinedAt = DateTime.UtcNow
            };

            session.Participants.TryAdd(userId, participant);
            _userSessions.TryAdd(userId, (chatId, Context.ConnectionId));

            // Cancel any pending timeout since we have a new participant
            CancelSessionTimeout(chatId);

            // Join SignalR group for this voice session
            await Groups.AddToGroupAsync(Context.ConnectionId, $"voice_{chatId}");

            // Notify others in the session
            await Clients.OthersInGroup($"voice_{chatId}").SendAsync("ParticipantJoined", participant);

            // Notify chat members that voice session is active (for UI indicators)
            await Clients.Group($"chat_{chatId}").SendAsync("VoiceSessionActive", new
            {
                ChatId = chatId,
                SessionId = session.Id,
                ParticipantCount = session.ParticipantCount
            });

            return session;
        }

        public async Task LeaveSession(Guid chatId)
        {
            var userId = GetUserId();

            if (!_activeSessions.TryGetValue(chatId, out var session))
                return;

            await RemoveParticipantFromSession(chatId, userId);
        }

        public Task<VoiceSessionState?> GetSession(Guid chatId)
        {
            _activeSessions.TryGetValue(chatId, out var session);
            return Task.FromResult(session);
        }

        public Task<Dictionary<Guid, int>> GetActiveSessionsForChats(List<Guid> chatIds)
        {
            var result = new Dictionary<Guid, int>();

            foreach (var chatId in chatIds)
            {
                if (_activeSessions.TryGetValue(chatId, out var session))
                {
                    result[chatId] = session.ParticipantCount;
                }
            }

            return Task.FromResult(result);
        }

        // ====== Participant State ======

        public async Task ToggleMute(Guid chatId)
        {
            var userId = GetUserId();

            if (!_activeSessions.TryGetValue(chatId, out var session))
                return;

            if (session.Participants.TryGetValue(userId, out var pariticipant))
            {
                pariticipant.IsMuted = !pariticipant.IsMuted;

                await Clients.Group($"voice_{chatId}").SendAsync("ParticipantMuteChanged", new
                {
                    UserId = userId,
                    IsMuted = pariticipant.IsMuted
                });
            }
        }

        public async Task ToggleDeafen(Guid chatId)
        {
            var userId = GetUserId();

            if (!_activeSessions.TryGetValue(chatId, out var session))
                return;

            if (session.Participants.TryGetValue(userId, out var participant))
            {
                participant.IsDeafened = !participant.IsDeafened;

                if (participant.IsDeafened)
                    participant.IsMuted = true;

                await Clients.Group($"voice_{chatId}").SendAsync("ParticipantMuteChanged", new
                {
                    UserId = userId,
                    IsDeafened = participant.IsDeafened,
                    isMuted = participant.IsMuted
                });
            }
        }

        public async Task SetMute(Guid chatId, bool isMuted)
        {
            var userId = GetUserId();

            if (!_activeSessions.TryGetValue(chatId, out var session))
                return;

            if (session.Participants.TryGetValue(userId, out var particiapnt))
            {
                if (!isMuted && particiapnt.IsDeafened)
                    return;

                particiapnt.IsMuted = isMuted;

                await Clients.Group($"voice_{chatId}").SendAsync("ParticipantMuteChanged", new
                {
                    UserId = userId,
                    IsMuted = particiapnt.IsMuted
                });
            }
        }

        public async Task StartSpeaking(Guid chatId)
        {
            var userId = GetUserId();

            if (!_activeSessions.TryGetValue(chatId, out var session))
                return;

            if (session.Participants.TryGetValue(userId, out var participant))
            {
                if (participant.IsMuted)
                    return;

                participant.IsSpeaking = true;

                await Clients.Group($"vouce_{chatId}").SendAsync("ParticipantStartedSpeaking", new
                {
                    UserId = userId
                });
            }
        }

        public async Task StopSpeaking(Guid chatId)
        {
            var userId = GetUserId();

            if (!_activeSessions.TryGetValue(chatId, out var session))
                return;

            if (session.Participants.TryGetValue(userId, out var participant))
            {
                participant.IsSpeaking = false;

                await Clients.Group($"vouce_{chatId}").SendAsync("ParticipantStartedSpeaking", new
                {
                    UserId = userId
                });
            }
        }

        // ====== WebRTC Signaling ======

        public async Task SendOffer(Guid chatId, Guid targetUserId, string offer)
        {
            if (!_activeSessions.TryGetValue(chatId, out var session))
                return;

            if (session.Participants.TryGetValue(targetUserId, out var targetParticipant))
            {
                await Clients.Client(targetParticipant.ConnectionId).SendAsync("ReceiveOffer", new
                {
                    FromUserId = GetUserId(),
                    FromUsername = GetUsername(),
                    Offer = offer
                });
            }
        }

        public async Task SendAnswer(Guid chatId, Guid targetUserId, string answer)
        {
            if (!_activeSessions.TryGetValue(chatId, out var session))
                return;

            if (session.Participants.TryGetValue(targetUserId, out var targetParticipant))
            {
                await Clients.Client(targetParticipant.ConnectionId).SendAsync("ReceiveAnswer", new
                {
                    FromUserId = GetUserId(),
                    Answer = answer
                });
            }
        }

        public async Task SendIceCandidate(Guid chatId, Guid targetUserId, string candidate)
        {
            if (!_activeSessions.TryGetValue(chatId, out var session))
            {
                return;
            }

            if (session.Participants.TryGetValue(targetUserId, out var targetParticipant))
            {
                await Clients.Client(targetParticipant.ConnectionId).SendAsync("ReceiveIceCandidate", new
                {
                    FromUserId = GetUserId(),
                    Candidate = candidate
                });
            }
        }

        // ====== Connection Lifecycle ======

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = GetUserId();

            if (_userSessions.TryGetValue(userId, out var sessionInfo))
            {
                await RemoveParticipantFromSession(sessionInfo.ChatId, userId);
            }

            await base.OnDisconnectedAsync(exception);
        }


        // ====== Private Helpers ======

        private async Task RemoveParticipantFromSession(Guid chatId, Guid userId)
        {
            if (!_activeSessions.TryGetValue(chatId, out var session))
                return;

            if (session.Participants.TryRemove(userId, out var removedParicipant))
            {
                _userSessions.TryRemove(userId, out _);

                await Groups.RemoveFromGroupAsync(removedParicipant.ConnectionId, $"voice_{chatId}");

                await Clients.Group($"voice_{chatId}").SendAsync("Paticipant left", new
                {
                    UserId = userId,
                    Username = removedParicipant.Username,
                    ParticipantCOunt = session.ParticipantCount
                });
            }

            if (session.IsEmpty)
            {
                await DestroySession(chatId, session);

            }
            else if (session.ParticipantCount == 1)
            {
                StartSessionTimeout(chatId, session);

                // Notify the remaining user about the timeout
                await Clients.Group($"voice_{chatId}").SendAsync("SessinTimeoutStarted", new
                {
                    ChatId = chatId,
                    TimeoutMinutes = SessionTimeout.TotalMinutes
                });

                // Update chat members
                await Clients.Group($"chat_{chatId}").SendAsync("VoiceSessionUpdated", new
                {
                    ChatId = chatId,
                    SessionId = session.Id,
                    ParticipantCount = session.ParticipantCount
                });
            }
            else
            {
                // Multiple participants remain, just update count
                await Clients.Group($"chat_{chatId}").SendAsync("VoiceSessionUpdated", new
                {
                    ChatId = chatId,
                    SessionId = session.Id,
                    ParticipantCount = session.ParticipantCount
                });
            }

        }


        private void StartSessionTimeout(Guid chatId, VoiceSessionState session)
        {
            CancelSessionTimeout(chatId);

            var cts = new CancellationTokenSource();
            _sessionTimeouts.TryAdd(chatId, cts);

            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(SessionTimeout, cts.Token);

                    if (_activeSessions.TryGetValue(chatId, out var currentSession) && currentSession.ParticipantCount <= 1)
                    {
                        await Clients.Group($"voice_{chatId}").SendAsync("SessionTimeoutExpired", new
                        {
                            ChatId = chatId
                        });

                        foreach (var pariticipant in currentSession.Participants.Values.ToList())
                        {
                            _userSessions.TryRemove(pariticipant.UserId, out _);
                            await Groups.RemoveFromGroupAsync(pariticipant.ConnectionId, $"voice_{chatId}");
                        }

                        await DestroySession(chatId, session);
                    }
                }
                catch (TaskCanceledException)
                {

                }
                finally
                {
                    _sessionTimeouts.TryRemove(chatId, out _);
                }
            });
        }

        private void CancelSessionTimeout(Guid chatId)
        {
            if (_sessionTimeouts.TryRemove(chatId, out var cts))
            {
                cts.Cancel();
                cts.Dispose();
            }
        }

        private async Task DestroySession(Guid chatId, VoiceSessionState session)
        {
            CancelSessionTimeout(chatId);
            _activeSessions.TryRemove(chatId, out _);

            await Clients.Group($"chat_{chatId}").SendAsync("VoiceSessionEnded", new
            {
                ChatId = chatId,
                SessionId = session.Id
            });
        }
    }
}
