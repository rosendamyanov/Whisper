using Whisper.Common.Response;
using Whisper.DTOs.Request.Friendship;
using Whisper.DTOs.Response.Friendship;

namespace Whisper.Services.Services.Interfaces
{
    public interface IFriendshipService
    {
        Task<ApiResponse<string>> AcceptFriendRequestAsync(Guid friendshipId, Guid userId);
        Task<ApiResponse<string>> DeclineFriendRequestAsync(Guid friendshipId, Guid userId);
        Task<ApiResponse<List<FriendResponseDto>>> GetFriendsAsync(Guid userId);
        Task<ApiResponse<FriendshipResponseDto>> GetFriendshipAsync(Guid userId, Guid friendId);
        Task<ApiResponse<List<FriendRequestDto>>> GetPendingRequestsAsync(Guid userId);
        Task<ApiResponse<List<FriendRequestDto>>> GetSentRequestsAsync(Guid userId);
        Task<ApiResponse<string>> RemoveFriendAsync(Guid friendshipId, Guid userId);
        Task<ApiResponse<string>> SendFriendRequestAsync(Guid userId, Guid friendId);
    }
}