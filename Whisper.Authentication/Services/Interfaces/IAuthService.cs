using Whisper.Authentication.DTOs.Request;
using Whisper.Authentication.DTOs.Response;
using Whisper.Common.Response;

namespace Whisper.Authentication.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResponse<AuthResponseDto>> Register(UserRegisterRequestDTO user);
        Task<ApiResponse<AuthResponseDto>> Login(UserLoginRequestDTO requestUser);
        Task<ApiResponse<string>> Logout(LogoutRequestDTO? body = null);
        Task<ApiResponse<AuthResponseDto>> RefreshToken(RefreshRequestDTO? refresh);
    }
}
