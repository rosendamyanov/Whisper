using Whisper.Authentication.DTOs.Request;
using Whisper.Authentication.DTOs.Response;
using Whisper.Common.Response;

namespace Whisper.Authentication.Services.Interfaces
{
    /// <summary>
    /// Provides authentication and authorization services including user registration, login, and token management
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Registers a new user account with email and password
        /// </summary>
        /// <param name="requestUser">User registration details including username, email, and password</param>
        /// <returns>ApiResponse containing JWT tokens on success or error details on failure</returns>
        /// <remarks>
        /// Validates:
        /// - Email format and uniqueness
        /// - Password strength requirements (minimum 8 characters, uppercase, lowercase, number, special char)
        /// - Username uniqueness
        /// 
        /// Uses BCrypt for password hashing with automatic salt generation.
        /// Creates both access token (15 min) and refresh token (7 days).
        /// Returns tokens in both HttpOnly cookies and JSON response body.
        /// </remarks>
        Task<ApiResponse<AuthResponseDto>> Register(UserRegisterRequestDTO requestUser);

        /// <summary>
        /// Authenticates a user with username/email and password
        /// </summary>
        /// <param name="requestUser">Login credentials (username or email, and password)</param>
        /// <returns>ApiResponse containing JWT tokens on success or error details on failure</returns>
        /// <remarks>
        /// Supports login with either username or email address.
        /// Verifies password using BCrypt hash comparison.
        /// Returns HttpOnly cookies + JSON tokens for dual-mode authentication.
        /// Access token expires in 15 minutes, refresh token in 7 days.
        /// </remarks>
        Task<ApiResponse<AuthResponseDto>> Login(UserLoginRequestDTO requestUser);

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
    }
}