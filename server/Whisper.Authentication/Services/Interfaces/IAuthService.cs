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
        /// Refreshes the user's authentication session using a valid refresh token, allowing persistent login without re-entering credentials.
        /// </summary>
        /// <param name="refresh">Optional DTO containing refresh token details. If null, the method attempts to extract tokens from HttpOnly cookies.</param>
        /// <returns>ApiResponse containing a new Access Token and a new Refresh Token.</returns>
        /// <remarks>
        /// <b>Authentication Logic:</b>
        /// <list type="bullet">
        /// <item>Identifies the user solely via the valid Refresh Token, enabling session recovery even after the Access Token has been deleted/expired.</item>
        /// <item>Implements <b>Refresh Token Rotation</b>: The old refresh token is revoked immediately, and a new pair is issued to prevent token theft.</item>
        /// </list>
        /// 
        /// <b>Validation Checks:</b>
        /// <list type="bullet">
        /// <item>Token ID exists in the database.</item>
        /// <item>Token matches the stored secure hash.</item>
        /// <item>Token is active (not revoked) and not expired.</item>
        /// <item>Associated user account exists.</item>
        /// </list>
        /// 
        /// Supports both JSON-based clients (Mobile/Desktop) and Cookie-based clients (Web) automatically.
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