using Whisper.Common.Response;
using Whisper.DTOs.Request.User;
using Whisper.DTOs.Response.User;

namespace Whisper.Services.Services.Interfaces
{
    public interface IUserService
    {
        Task<ApiResponse<UserSessionResponseDto>> Login(UserLoginRequestDTO requestUser);
        Task<ApiResponse<UserSessionResponseDto>> Register(UserRegisterRequestDTO requestUser);
        Task<ApiResponse<List<UserBasicDto>>> FindUsersByUsername(string query);
    }
}