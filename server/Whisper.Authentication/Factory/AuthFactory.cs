using Whisper.Authentication.Factory.Interfaces;
using Whisper.DTOs.Response.Auth;
using Whisper.DTOs.Request.User;
using Whisper.Data.Models;
using Whisper.Data.Models.Authentication;
using Whisper.Common.Response.Authentication;

namespace Whisper.Authentication.Factory
{
    public class AuthFactory : IAuthFactory
    {
        public RevokedToken Map(RefreshToken refreshToken)
        {
            RevokedToken revokedToken = new RevokedToken()
            {
                Id = Guid.NewGuid(),
                TokenHash = refreshToken.TokenHash,
                RevokedAt = DateTime.UtcNow,
                Reason = AuthMessages.TokenRefreshed,
                UserId = refreshToken.UserId,
                User = refreshToken.User
            };
            return revokedToken;
        }

        public AuthResponseDto Map(string accessToken, string rawRefreshToken, Guid refreshTokenId, DateTime expiryDate)
        {
            AuthResponseDto tokens = new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = rawRefreshToken,
                RefreshTokenId = refreshTokenId,
                AccessTokenExpiry = expiryDate
            };
            return tokens;
        }
    }
}
