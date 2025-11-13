using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Whisper.Authentication.Configuration;
using Whisper.Authentication.Factory.Interfaces;
using Whisper.Authentication.Services.Interfaces;
using Whisper.Authentication.Validation.Interfaces;
using Whisper.Common.Response;
using Whisper.Common.Response.Authentication;
using Whisper.Authentication.Data.Interfaces;
using Whisper.Authentication.DTOs.Request;
using Whisper.Authentication.DTOs.Response;
using Whisper.Data.Models;
using Whisper.Data.Models.Authentication;

namespace Whisper.Authentication.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IAuthFactory _authFactory;
        private readonly IEmailValidation _emailValidation;
        private readonly IPasswordValidation _passwordValidation;
        private readonly ITokenService _tokenService;
        private readonly IOptions<JwtSettings> _jwtSettings;
        public AuthService(
            IAuthRepository userRepository,
            IAuthFactory userFactory,
            IEmailValidation emailValidation,
            IPasswordValidation passwordValidation,
            ITokenService tokenService,
            IOptions<JwtSettings> jwtSettings)
        {
            _authRepository = userRepository;
            _authFactory = userFactory;
            _emailValidation = emailValidation;
            _passwordValidation = passwordValidation;
            _tokenService = tokenService;
            _jwtSettings = jwtSettings;

        }

        //TO DO:
        //2. Logout/Token Revocation (POST /auth/logout)
        //3. Password Reset Flow (request + confirm endpoints)
        //4. Email Verification (POST /auth/verify-email)
        //5. Session Management (list/revoke sessions)
        //6. Two-Factor Authentication (enable/verify)
        //8. Validation if the email domain is reachable - SignalR
        //9. Logging Erros in .log files or app console for easier debugging and monitoring
        //10. Implement memory caching for username, email and tokens(to reduce db calls).

        public async Task<ApiResponse<AuthResponseDto>> Register(UserRegisterRequestDTO requestUser)
        {
            if (!_emailValidation.IsEmailValid(requestUser.Email))
            {
                return ApiResponse<AuthResponseDto>.Failure(ResponseMessages.InvalidEmail, ResponseCodes.InvalidEmail);
            }

            if (!_passwordValidation.IsStrong(requestUser.Password))
            {
                return ApiResponse<AuthResponseDto>.Failure(ResponseMessages.WeakPassword, ResponseCodes.WeakPassword);
            }

            (bool usernameExists, bool emailExists) = await _authRepository.CheckUserExistenceAsync(requestUser.Username, requestUser.Email);

            switch (true)
            {
                case true when usernameExists:
                    return ApiResponse<AuthResponseDto>.Failure(ResponseMessages.UsernameExists, ResponseCodes.UsernameExists);
                case true when emailExists:
                    return ApiResponse<AuthResponseDto>.Failure(ResponseMessages.EmailExists, ResponseCodes.EmailExists);
                case true when usernameExists && emailExists:
                    return ApiResponse<AuthResponseDto>.Failure(ResponseMessages.UsernameAndEmailExists, ResponseCodes.UsernameAndEmailExists);
            }

            string passwordHash = requestUser.Password = BCrypt.Net.BCrypt.HashPassword(requestUser.Password);
            (User user, UserCredentials credentials) = _authFactory.Map(requestUser, passwordHash);

            using var transaction = await _authRepository.BeginTransactionAsync();

            bool isRegistrationSuccess = await _authRepository.AddUserAsync(user);

            if (!isRegistrationSuccess)
            {
                await transaction.RollbackAsync();
                return ApiResponse<AuthResponseDto>.Failure(ResponseMessages.RegistrationFailed, ResponseCodes.RegistrationFailed);
            }

            bool isCredentialsSuccess = await _authRepository.SaveUserCredetialsAsync(credentials);

            if (!isCredentialsSuccess)
            {
                await transaction.RollbackAsync();
                return ApiResponse<AuthResponseDto>.Failure(ResponseMessages.CredentialsSaveFailed, ResponseCodes.CredentialsSaveFailed);
            }

            await transaction.CommitAsync();

            AuthResponseDto tokens = await GenerateAndAttachTokens(user);

            return ApiResponse<AuthResponseDto>.Success(tokens, ResponseMessages.UserRegistered);
        }

        public async Task<ApiResponse<AuthResponseDto>> Login(UserLoginRequestDTO requestUser)
        {
            User? user = await _authRepository.GetUserWithCredentialsByIdentifierAsync(requestUser.Username);

            if (user == null || user.Credentials == null)
            {
                return ApiResponse<AuthResponseDto>.Failure(ResponseMessages.UserNotFound, ResponseCodes.UserNotFound);
            }

            if (!BCrypt.Net.BCrypt.Verify(requestUser.Password, user.Credentials.PasswordHash))
            {
                return ApiResponse<AuthResponseDto>.Failure(ResponseMessages.InvalidCredentials, ResponseCodes.InvalidCredentials);
            }

            AuthResponseDto tokens = await GenerateAndAttachTokens(user);

            await _authRepository.SaveUserRefreshTokenAsync(user);

            return ApiResponse<AuthResponseDto>.Success(tokens, ResponseMessages.UserLogged);
        }

        public async Task<ApiResponse<AuthResponseDto>> RefreshToken(RefreshRequestDto refresh)
        {
            ClaimsPrincipal principal;

            try
            {
                principal = _tokenService.GetPrincipalFromToken(refresh.AccessToken);
            }
            catch (SecurityTokenException)
            {
                return ApiResponse<AuthResponseDto>.Failure(ResponseMessages.InvalidAccessToken, ResponseCodes.InvalidAccessToken);
            }

            var username = principal.Identity?.Name;

            if (username == null)
            {
                return ApiResponse<AuthResponseDto>.Failure(ResponseMessages.UsernameNotFound, ResponseCodes.UsernameNotFound);
            }

            User? user = await _authRepository.GetUserRefreshTokenAsync(username);

            if (user == null)
            {
                return ApiResponse<AuthResponseDto>.Failure(ResponseMessages.UserNotFound, ResponseCodes.UserNotFound);
            }

            RefreshToken? storedRefreshToken = await _authRepository.GetRefreshTokenByIdAsync(refresh.RefreshTokenId, user.Id);

            if (storedRefreshToken == null || storedRefreshToken.IsRevoked || storedRefreshToken.ExpiresAt <= DateTime.UtcNow)
            {
                return ApiResponse<AuthResponseDto>.Failure(ResponseMessages.RefreshIsRevokedOrExpired, ResponseCodes.RefreshIsRevokedOrExpired);
            }

            if (!BCrypt.Net.BCrypt.Verify(refresh.RefreshToken, storedRefreshToken.TokenHash))
            {
                return ApiResponse<AuthResponseDto>.Failure(ResponseMessages.RefreshNotFound, ResponseCodes.RefreshNotFound);
            }

            storedRefreshToken.IsRevoked = true;

            RevokedToken revokedToken = _authFactory.Map(storedRefreshToken);

            bool result = await _authRepository.SaveRevokedRefreshTokenAsync(revokedToken, storedRefreshToken);

            if (!result)
            {
                return ApiResponse<AuthResponseDto>.Failure(ResponseMessages.RefreshRevokeFailed, ResponseCodes.RefreshRevokeFailed);
            }

            AuthResponseDto tokens = await GenerateAndAttachTokens(user);

            return ApiResponse<AuthResponseDto>.Success(tokens, ResponseMessages.TokenRefreshed);
        }

        private async Task<AuthResponseDto> GenerateAndAttachTokens(User user)
        {
            string accessToken = _tokenService.GenerateAccessToken(user);
            (string rawRefreshToken, RefreshToken refreshToken) = _tokenService.GenerateRefreshToken();


            refreshToken.UserId = user.Id;
            await _authRepository.SaveRefreshTokenAsync(refreshToken);

            AuthResponseDto tokens = _authFactory.Map(
                accessToken,
                rawRefreshToken,
                refreshToken.Id,
                DateTime.UtcNow.AddMinutes(_jwtSettings.Value.AccessTokenExpirationMinutes));

            return tokens;
        }
    }
}
