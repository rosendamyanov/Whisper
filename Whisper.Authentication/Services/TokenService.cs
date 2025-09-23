using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Whisper.Authentication.Configuration;
using Whisper.Authentication.Services.Interfaces;
using Whisper.Data.Models;
using Whisper.Data.Models.Authentication;

namespace Whisper.Authentication.Services
{
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _settings;

        public TokenService(IOptions<JwtSettings> settings)
            => _settings = settings.Value;

        public string GenerateAccessToken(User user)
        {
            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _settings.Issuer,
                audience: _settings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpirationMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public (string RawToken, RefreshToken HashedToken) GenerateRefreshToken()
        {
            var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            var hashedToken = BCrypt.Net.BCrypt.HashPassword(rawToken);

            return (
                RawToken: rawToken,
                HashedToken: new RefreshToken
                {
                    TokenHash = hashedToken,
                    ExpiresAt = DateTime.UtcNow.AddDays(_settings.RefreshTokenExpirationDays),
                    CreatedAt = DateTime.UtcNow,
                    IsRevoked = false
                }
            );
        }

        public bool ValidateRefreshToken(string rawToken, RefreshToken storedToken)
        {
            return BCrypt.Net.BCrypt.Verify(rawToken, storedToken.TokenHash);
        }

        public ClaimsPrincipal GetPrincipalFromToken(string token)
        {
            var validator = new JwtSecurityTokenHandler();
            var validationParams = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret)),
                ValidateIssuer = true,
                ValidIssuer = _settings.Issuer,
                ValidateAudience = true,
                ValidAudience = _settings.Audience,
                ValidateLifetime = false // For expired token validation
            };

            return validator.ValidateToken(token, validationParams, out _);
        }
    }
}
