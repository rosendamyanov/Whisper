using Whisper.Common.Response;
using Whisper.DTOs.Response.Friendship;
using Whisper.DTOs.Request.Friendship;
using Whisper.Data.Repositories.Interfaces;
using Whisper.Services.Factories.Interfaces;
using Whisper.Services.Services.Interfaces;

namespace Whisper.Services.Services
{
    public class FriendshipService : IFriendshipService
    {
        private readonly IFriendshipRepository _friendshipRepository;
        private readonly IFriendshipFactory _friendshipFactory;
        public FriendshipService(IFriendshipRepository friendshipRepository, IFriendshipFactory friendshipFactory)
        {
            _friendshipRepository = friendshipRepository;
            _friendshipFactory = friendshipFactory;
        }

        // ====== Queries ======
        public async Task<ApiResponse<FriendshipResponseDto>> GetFriendshipAsync(Guid userId, Guid friendId)
        {
            var friendship = await _friendshipRepository.GetFriendshipAsync(userId, friendId);

            if (friendship == null)
                return ApiResponse<FriendshipResponseDto>.Failure(FriendshipMessages.FriendshipNotFound, FriendshipCodes.FriendshipNotFound);

            var response = _friendshipFactory.ToDto(friendship, userId);

            return ApiResponse<FriendshipResponseDto>.Success(response);
        }

        public async Task<ApiResponse<List<FriendResponseDto>>> GetFriendsAsync(Guid userId)
        {
            var friendships = await _friendshipRepository.GetFriendsAsync(userId);

            var response = friendships.Select(f => _friendshipFactory.ToFriendDto(f, userId)).ToList();

            return ApiResponse<List<FriendResponseDto>>.Success(response);
        }

        public async Task<ApiResponse<List<FriendRequestDto>>> GetPendingRequestsAsync(Guid userId)
        {
            var requests = await _friendshipRepository.GetPendingRequestsAsync(userId);

            var response = requests.Select(f => _friendshipFactory.ToPendingRequestDto(f)).ToList();

            return ApiResponse<List<FriendRequestDto>>.Success(response);
        }

        public async Task<ApiResponse<List<FriendRequestDto>>> GetSentRequestsAsync(Guid userId)
        {
            var requests = await _friendshipRepository.GetSentRequestsAsync(userId);

            var response = requests.Select(f => _friendshipFactory.ToSentRequestDto(f)).ToList();

            return ApiResponse<List<FriendRequestDto>>.Success(response);
        }

        // ====== Actions ======
        public async Task<ApiResponse<string>> SendFriendRequestAsync(Guid userId, Guid friendId)
        {
            if (userId == friendId)
                return ApiResponse<string>.Failure(FriendshipMessages.CannotFriendSelf, FriendshipCodes.CannotFriendSelf);

            var exists = await _friendshipRepository.FriendshipExistsAsync(userId, friendId);
            if (exists)          
                return ApiResponse<string>.Failure(FriendshipMessages.FriendshipAlreadyExists, FriendshipCodes.FriendshipAlreadyExists);

            var friendship = _friendshipFactory.Create(userId, friendId);

            var success = await _friendshipRepository.SendFriendRequestAsync(friendship);

            return success
                ? ApiResponse<string>.Success(FriendshipMessages.FriendRequestSent)
                : ApiResponse<string>.Failure(FriendshipMessages.SendFriendRequestFailed, FriendshipCodes.SendFriendRequestFailed);
        }

        public async Task<ApiResponse<string>> AcceptFriendRequestAsync(Guid friendshipId, Guid userId)
        {
            var success = await _friendshipRepository.AcceptFriendRequestAsync(friendshipId, userId);
            return success
                ? ApiResponse<string>.Success(FriendshipMessages.FriendRequestAccepted)
                : ApiResponse<string>.Failure(FriendshipMessages.AcceptFriendRequestFailed, FriendshipCodes.AcceptFriendRequestFailed);
        }

        public async Task<ApiResponse<string>> DeclineFriendRequestAsync(Guid friendshipId, Guid userId)
        {
            var success = await _friendshipRepository.DeclineFriendRequestAsync(friendshipId, userId);
            return success
                ? ApiResponse<string>.Success(FriendshipMessages.FriendRequestDeclined)
                : ApiResponse<string>.Failure(FriendshipMessages.DeclineFriendRequestFailed, FriendshipCodes.DeclineFriendRequestFailed);
        }

        public async Task<ApiResponse<string>> RemoveFriendAsync(Guid friendshipId, Guid userId)
        {
            var success = await _friendshipRepository.RemoveFriendAsync(friendshipId, userId);
            return success
                ? ApiResponse<string>.Success(FriendshipMessages.FriendRemoved)
                : ApiResponse<string>.Failure(FriendshipMessages.RemoveFriendFailed, FriendshipCodes.RemoveFriendFailed);
        }
    }
}
