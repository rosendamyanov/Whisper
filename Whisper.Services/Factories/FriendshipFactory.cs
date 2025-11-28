using Whisper.Data.Models;
using Whisper.DTOs.Request.Friendship;
using Whisper.DTOs.Response.Friendship;
using Whisper.Services.Factories.Interfaces;

namespace Whisper.Services.Factories
{
    public class FriendshipFactory : IFriendshipFactory
    {
        public FriendshipResponseDto ToDto(Friendship friendship, Guid userId)
        {
            var friend = friendship.UserId == userId ? friendship.Friend : friendship.User;

            return new FriendshipResponseDto
            {
                Id = friendship.Id,
                Friend = new FriendResponseDto
                {
                    Id = friend.Id,
                    FriendshipId = friendship.Id,
                    Username = friend.Username,
                    FriendsSince = friendship.AcceptedAt
                },
                IsAccepted = friendship.IsAccepted,
                CreatedAt = friendship.CreatedAt,
                AcceptedAt = friendship.AcceptedAt
            };
        }

        public Friendship Create(Guid userId, Guid friendId)
        {
            return new Friendship
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                FriendId = friendId,
                CreatedAt = DateTime.UtcNow
            };
        }

        public FriendResponseDto ToFriendDto(Friendship friendship, Guid currentUserId)
        {
            var friend = friendship.UserId == currentUserId ? friendship.Friend : friendship.User;
            return new FriendResponseDto
            {
                Id = friend.Id,
                FriendshipId = friendship.Id,
                Username = friend.Username,
                FriendsSince = friendship.AcceptedAt
            };
        }

        public FriendRequestDto ToPendingRequestDto(Friendship friendship)
        {
            return new FriendRequestDto
            {
                FriendshipId = friendship.Id,
                UserId = friendship.User.Id,
                Username = friendship.User.Username,
                SentAt = friendship.CreatedAt
            };
        }

        public FriendRequestDto ToSentRequestDto(Friendship friendship)
        {
            return new FriendRequestDto
            {
                FriendshipId = friendship.Id,
                UserId = friendship.Friend.Id,
                Username = friendship.Friend.Username,
                SentAt = friendship.CreatedAt
            };
        }
    }
}
