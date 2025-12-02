using Whisper.Data.Models;

namespace Whisper.Data.Repositories.Interfaces
{
    public interface IFriendshipRepository
    {
        // Queries
        Task<Friendship?> GetFriendshipAsync(Guid userId, Guid friendId);
        Task<List<Friendship>> GetFriendsAsync(Guid userId);
        Task<List<Friendship>> GetPendingRequestsAsync(Guid userId);
        Task<List<Friendship>> GetSentRequestsAsync(Guid userId);
        Task<bool> FriendshipExistsAsync(Guid userId, Guid friendId);

        // Actions
        Task<bool> SendFriendRequestAsync(Friendship friendship);
        Task<bool> AcceptFriendRequestAsync(Guid friendshipId, Guid userId);
        Task<bool> DeclineFriendRequestAsync(Guid friendshipId, Guid userId);
        Task<bool> RemoveFriendAsync(Guid friendshipId, Guid userId);
    }
}
