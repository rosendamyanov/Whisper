using Whisper.Authentication.DTOs.Request;
using Whisper.Authentication.DTOs.Response;
using Whisper.Data.Models;
using Whisper.Data.Models.Authentication;
namespace Whisper.Authentication.Factory.Interfaces
{
    public interface IAuthFactory
    {
        (User user, UserCredentials credentials) Map(UserRegisterRequestDTO request, string passwordHash);
        RevokedToken Map(RefreshToken refreshToken);
        AuthResponseDto Map(string accessToken, string rawRefreshToken, Guid refreshTokenId, DateTime expiryDate);

    }
}
