using Whisper.Authentication.Factory.Interfaces;
using Whisper.Authentication.DTOs.Request;
using Whisper.Authentication.DTOs.Response;
using Whisper.Data.Models;
using Whisper.Data.Models.Authentication;
using Whisper.Common.Response.Authentication;
{
    
}

namespace Whisper.Authentication.Factory
{
    public class AuthFactory : IAuthFactory
    {
        public (User user, UserCredentials credentials) Map(UserRegisterRequestDTO request, string passwordHash)
        {
            User user = new User()
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                Email = request.Email,
                CreatedAt = DateTime.UtcNow,
                Role = "User",
                RefreshTokens = new List<RefreshToken>()
            };

            UserCredentials credentials = new UserCredentials
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                PasswordHash = passwordHash,
                CreatedAt = DateTime.UtcNow
            };
            return (user, credentials);
        }

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
