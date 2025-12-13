using Microsoft.AspNetCore.Http;
using Whisper.Common.Response;
using Whisper.Data.Models;
using Whisper.Data.Models.Authentication;
using Whisper.DTOs.Request.Auth;
using Whisper.DTOs.Request.User;
using Whisper.DTOs.Response.Auth;

namespace Whisper.Authentication.Services.Interfaces
{
    /// <summary>
    /// Provides authentication and authorization services including user registration, login, and token management
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Generates a new access token using a valid refresh token
        /// </summary>
        /// <param name="refresh">Optional refresh token details (can use cookies if not provided)</param>
        /// <returns>ApiResponse containing new JWT tokens or error details</returns>
        /// <remarks>
        /// Implements refresh token rotation for enhanced security:
        /// - Old refresh token is revoked immediately
        /// - New refresh token pair is issued
        /// - Prevents replay attacks with stolen tokens
        /// 
        /// Validates:
        /// - Refresh token exists in database
        /// - Token hasn't been revoked
        /// - Token hasn't expired
        /// - Token hash matches stored value
        /// 
        /// Uses sliding expiration to maintain security while keeping users logged in.
        /// Accepts tokens from either request body or HttpOnly cookies.
        /// </remarks>
        Task<ApiResponse<AuthResponseDto>> RefreshToken(RefreshRequestDTO? refresh = null);

        /// <summary>
        /// Revokes the current user's refresh token and clears authentication cookies
        /// </summary>
        /// <param name="body">Optional logout details (can use cookies if not provided)</param>
        /// <returns>ApiResponse with success message or error details</returns>
        /// <remarks>
        /// Supports token input from both request body (for Swagger/API clients) and cookies (for web clients).
        /// Revoked tokens are stored in database to prevent reuse and detect token theft.
        /// Clears all authentication cookies (AccessToken, RefreshToken, RefreshTokenId).
        /// </remarks>
        Task<ApiResponse<string>> Logout(LogoutRequestDTO? body = null);
        Task<RefreshToken?> GetRefreshTokenByIdAsync(Guid refreshId);
        Task<AuthResponseDto> GenerateAndAttachTokens(User user);
        Task<bool> RevokeRefreshToken(RefreshToken refreshToken);
        public void DeleteAuthCookies(HttpResponse response);
    }
}