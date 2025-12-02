using Whisper.Data.Models;
using Whisper.DTOs.Request.Friendship;
using Whisper.DTOs.Response.Friendship;

namespace Whisper.Services.Factories.Interfaces
{
    public interface IFriendshipFactory
    {
        Friendship Create(Guid userId, Guid friendId);
        FriendshipResponseDto ToDto(Friendship friendship, Guid userId);
        FriendResponseDto ToFriendDto(Friendship friendship, Guid currentUserId);
        FriendRequestDto ToPendingRequestDto(Friendship friendship);
        FriendRequestDto ToSentRequestDto(Friendship friendship);
    }
}