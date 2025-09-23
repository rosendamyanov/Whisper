using System.Security.Claims;
using Whisper.Data.Models;
using Whisper.Data.Models.Authentication;

namespace Whisper.Authentication.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        (string RawToken, RefreshToken HashedToken) GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromToken(string token);
        bool ValidateRefreshToken(string rawToken, RefreshToken storedToken);
    }
}
