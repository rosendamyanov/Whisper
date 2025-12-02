using System.Security.Claims;
using Whisper.Data.Models;
using Whisper.Data.Models.Authentication;

namespace Whisper.Authentication.Services.Interfaces
{
    /// <summary>
    /// Provides JWT token generation and validation services
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Generates a JWT access token containing user claims
        /// </summary>
        /// <param name="user">User to generate token for</param>
        /// <returns>Signed JWT token string</returns>
        /// <remarks>
        /// Token includes claims:
        /// - Sub (Subject): User ID
        /// - UniqueName: Username
        /// - Email: User email address
        /// - Role: User role (e.g., "User", "Admin")
        /// 
        /// Token expires after configured AccessTokenExpirationMinutes (default: 15 minutes).
        /// Uses HMAC-SHA256 signing algorithm.
        /// </remarks>
        string GenerateAccessToken(User user);

        /// <summary>
        /// Generates a cryptographically secure refresh token
        /// </summary>
        /// <returns>Tuple containing raw token (to send to client) and hashed token entity (to store in database)</returns>
        /// <remarks>
        /// Generates a 64-byte random token using cryptographically secure RNG.
        /// Token is hashed with BCrypt before storage to prevent database compromise attacks.
        /// Expires after configured RefreshTokenExpirationDays (default: 7 days).
        /// </remarks>
        (string RawToken, RefreshToken HashedToken) GenerateRefreshToken();

        /// <summary>
        /// Validates a raw refresh token against its stored hash
        /// </summary>
        /// <param name="rawToken">Raw token received from client</param>
        /// <param name="storedToken">Hashed token from database</param>
        /// <returns>True if token is valid, false otherwise</returns>
        bool ValidateRefreshToken(string rawToken, RefreshToken storedToken);

        /// <summary>
        /// Extracts claims from a JWT token without validating its lifetime
        /// </summary>
        /// <param name="token">JWT token string</param>
        /// <returns>ClaimsPrincipal containing user claims</returns>
        /// <remarks>
        /// Used for refresh token flow where we need to extract user identity from an expired access token.
        /// Validates signature, issuer, and audience but ignores expiration time.
        /// </remarks>
        /// <exception cref="SecurityTokenException">Thrown if token signature or format is invalid</exception>
        ClaimsPrincipal GetPrincipalFromToken(string token);
    }
}