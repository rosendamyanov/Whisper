using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Whisper.Authentication.Configuration;
using Whisper.Authentication.Data.Interfaces;
using Whisper.Authentication.Factory.Interfaces;
using Whisper.Authentication.Services.Interfaces;
using Whisper.Common.Response;
using Whisper.Common.Response.Authentication;
using Whisper.Data.Models;
using Whisper.Data.Models.Authentication;
using Whisper.DTOs.Request.Auth;
using Whisper.DTOs.Request.User;
using Whisper.DTOs.Response.Auth;

namespace Whisper.Authentication.Services
{
    /// <summary>
    /// Implements authentication business logic with JWT-based token management and dual-mode delivery (cookies + JSON)
    /// </summary>
    public class AuthService : IAuthService
    {
        private const string AccessTokenCookie = "AccessToken";
        private const string RefreshTokenCookie = "RefreshToken";
        private const string RefreshTokenIdCookie = "RefreshTokenId";

        private readonly IAuthRepository _authRepository;
        private readonly IAuthFactory _authFactory;
        private readonly ITokenService _tokenService;
        private readonly IOptions<JwtSettings> _jwtSettings;
        private readonly IHttpContextAccessor _httpContext;
        public AuthService(
            IAuthRepository userRepository,
            IAuthFactory userFactory,
            ITokenService tokenService,
            IOptions<JwtSettings> jwtSettings,
            IHttpContextAccessor httpContext)
        {
            _authRepository = userRepository;
            _authFactory = userFactory;
            _tokenService = tokenService;
            _jwtSettings = jwtSettings;
            _httpContext = httpContext;
        }

        //TO DO:
        //3. Password Reset Flow (request + confirm endpoints)
        //4. Email Verification (POST /auth/verify-email)
        //5. Session Management (list/revoke sessions)
        //6. Two-Factor Authentication (enable/verify)
        //8. Validation if the email domain is reachable - SignalR
        //9. Logging Erros in .log files or app console for easier debugging and monitoring
        //10. Implement memory caching for username, email and tokens(to reduce db calls).


        public async Task<RefreshToken?>GetRefreshTokenByIdAsync(Guid refreshId)
        {
            RefreshToken? refreshToken = await _authRepository.GetRefreshTokenByIdAsync(refreshId, null);
            return refreshToken;
        }

        /// <inheritdoc />
        public async Task<ApiResponse<string>> Logout(LogoutRequestDTO? body = null)
        {
            var request = _httpContext.HttpContext!.Request;
            var response = _httpContext.HttpContext.Response;

            string? refreshIdStr = body?.RefreshTokenId?.ToString() ?? request.Cookies[RefreshTokenIdCookie];
            string? refreshTokenRaw = body?.RefreshToken ?? request.Cookies[RefreshTokenCookie];

            if (string.IsNullOrEmpty(refreshIdStr) || string.IsNullOrEmpty(refreshTokenRaw))
                return ApiResponse<string>.Failure(AuthMessages.TokensMissing, AuthCodes.TokensMissing);

            if (!Guid.TryParse(refreshIdStr, out Guid refreshId))
                return ApiResponse<string>.Failure(AuthMessages.InvalidRefreshTokenId, AuthCodes.InvalidRefreshTokenId);

            RefreshToken? stored = await _authRepository.GetRefreshTokenByIdAsync(refreshId, null);

            if (stored != null)
            {
                bool revoked = await RevokeRefreshToken(stored);
                if (!revoked)
                    return ApiResponse<string>.Failure(AuthMessages.FailedToRevokeToken, AuthCodes.InvalidRefreshTokenId);
            }

            DeleteAuthCookies(response);

            return ApiResponse<string>.Success(AuthMessages.LoggedOut);
        }

        /// <inheritdoc />
        public async Task<ApiResponse<AuthResponseDto>> RefreshToken(RefreshRequestDTO? refresh = null)
        {
            string? accessToken = refresh?.AccessToken;
            string? refreshTokenRaw = refresh?.RefreshToken;
            Guid? refreshTokenId = refresh?.RefreshTokenId;

            if (accessToken == null || refreshTokenRaw == null || refreshTokenId == null)
            {
                var request = _httpContext.HttpContext!.Request;
                accessToken ??= request.Cookies[AccessTokenCookie];
                refreshTokenRaw ??= request.Cookies[RefreshTokenCookie];

                var refreshIdStr = request.Cookies[RefreshTokenIdCookie];
                if (refreshTokenId == null && Guid.TryParse(refreshIdStr, out Guid parsed))
                    refreshTokenId = parsed;
            }

            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(refreshTokenRaw) || refreshTokenId == null)
                return ApiResponse<AuthResponseDto>.Failure(AuthMessages.TokensMissing, AuthCodes.TokensMissing);


            ClaimsPrincipal principal;
            try
            {
                principal = _tokenService.GetPrincipalFromToken(accessToken);
            }
            catch (SecurityTokenException)
            {
                return ApiResponse<AuthResponseDto>.Failure(AuthMessages.InvalidAccessToken, AuthCodes.InvalidAccessToken);
            }

            var username = principal.Identity?.Name;
            if (username == null)
                return ApiResponse<AuthResponseDto>.Failure(AuthMessages.UsernameNotFound, AuthCodes.UsernameNotFound);

            User? user = await _authRepository.GetUserRefreshTokenAsync(username);
            if (user == null)
                return ApiResponse<AuthResponseDto>.Failure(AuthMessages.UserNotFound, AuthCodes.UserNotFound);

            RefreshToken? storedRefreshToken = await _authRepository.GetRefreshTokenByIdAsync(refreshTokenId.Value, user.Id);
            if (storedRefreshToken == null || storedRefreshToken.IsRevoked || storedRefreshToken.ExpiresAt <= DateTime.UtcNow)
                return ApiResponse<AuthResponseDto>.Failure(AuthMessages.RefreshIsRevokedOrExpired, AuthCodes.RefreshIsRevokedOrExpired);

            if (!BCrypt.Net.BCrypt.Verify(refreshTokenRaw, storedRefreshToken.TokenHash))
                return ApiResponse<AuthResponseDto>.Failure(AuthMessages.RefreshNotFound, AuthCodes.RefreshNotFound);


            bool revokeResult = await RevokeRefreshToken(storedRefreshToken);
            if (!revokeResult)
                return ApiResponse<AuthResponseDto>.Failure(AuthMessages.RefreshRevokeFailed, AuthCodes.RefreshRevokeFailed);


            AuthResponseDto newTokens = await GenerateAndAttachTokens(user);
            return ApiResponse<AuthResponseDto>.Success(newTokens, AuthMessages.TokenRefreshed);
        }

        /// <summary>
        /// Generates JWT access and refresh tokens, saves refresh token to database, and attaches as HttpOnly cookies
        /// </summary>
        /// <param name="user">User to generate tokens for</param>
        /// <returns>AuthResponseDto containing both tokens and expiration time</returns>
        public async Task<AuthResponseDto> GenerateAndAttachTokens(User user)
        {
            string accessToken = _tokenService.GenerateAccessToken(user);
            (string rawRefreshToken, RefreshToken refreshToken) = _tokenService.GenerateRefreshToken();


            refreshToken.UserId = user.Id;
            await _authRepository.SaveRefreshTokenAsync(refreshToken);

            var response = _httpContext.HttpContext!.Response;

            response.Cookies.Append(AccessTokenCookie, 
                                    accessToken, 
                                    CookieConfig.Build(_jwtSettings.Value.AccessTokenExpirationMinutes));
            response.Cookies.Append(RefreshTokenCookie, 
                                    rawRefreshToken, 
                                    CookieConfig.Build(_jwtSettings.Value.RefreshTokenExpirationDays * 24 * 60));
            response.Cookies.Append(RefreshTokenIdCookie, 
                                    refreshToken.Id.ToString(), 
                                    CookieConfig.Build(_jwtSettings.Value.RefreshTokenExpirationDays * 24 * 60));

            AuthResponseDto tokens = _authFactory.Map(
                accessToken,
                rawRefreshToken,
                refreshToken.Id,
                DateTime.UtcNow.AddMinutes(_jwtSettings.Value.AccessTokenExpirationMinutes));

            return tokens;
        }

        /// <summary>
        /// Marks a refresh token as revoked and stores it in the revoked tokens table
        /// </summary>
        /// <param name="refreshToken">The refresh token to revoke</param>
        /// <returns>True if revocation succeeded, false otherwise</returns>
        public async Task<bool> RevokeRefreshToken(RefreshToken refreshToken)
        {
            refreshToken.IsRevoked = true;
            RevokedToken revokedToken = _authFactory.Map(refreshToken);
    
            return await _authRepository.SaveRevokedRefreshTokenAsync(revokedToken, refreshToken);
        }

        /// <summary>
        /// Deletes all authentication cookies from the response
        /// </summary>
        /// <param name="response">HTTP response to delete cookies from</param>
        public void DeleteAuthCookies(HttpResponse response)
        {
            response.Cookies.Delete(AccessTokenCookie);
            response.Cookies.Delete(RefreshTokenCookie);
            response.Cookies.Delete(RefreshTokenIdCookie);
        }
    }
}
