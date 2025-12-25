using Whisper.Services.Services.Interfaces;

namespace Whisper.Services.Services
{
    public class PresenceTracker : IPresenceTracker
    {
        private static readonly Dictionary<Guid, HashSet<string>> OnlineUsers = new();

        public Task<bool> UserConnected(Guid userId, string connectionId)
        {
            bool isOnline = false;

            lock (OnlineUsers)
            {
                if (OnlineUsers.ContainsKey(userId))
                {
                    OnlineUsers[userId].Add(connectionId);
                }
                else
                {
                    OnlineUsers[userId] = new HashSet<string> { connectionId };
                    isOnline = true;
                }
            }
            return Task.FromResult(isOnline);
        }

        public Task<bool> UserDisconnected(Guid userId, string connectionId)
        {
            bool isOffline = false;
            lock (OnlineUsers)
            {
                if (!OnlineUsers.ContainsKey(userId))
                    return Task.FromResult(isOffline);

                OnlineUsers[userId].Remove(connectionId);

                if (OnlineUsers[userId].Count == 0)
                {
                    OnlineUsers.Remove(userId);
                    isOffline = true;
                }
            }
            return Task.FromResult(isOffline);

        }
    }
}
