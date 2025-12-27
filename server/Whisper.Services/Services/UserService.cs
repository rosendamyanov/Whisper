using Whisper.Authentication.Services.Interfaces;
using Whisper.Common.Response;
using Whisper.Common.Response.Authentication;
using Whisper.Data.Models;
using Whisper.Data.Models.Authentication;
using Whisper.Data.Repositories.Interfaces;
using Whisper.DTOs.Response.User;
using Whisper.DTOs.Request.User;
using Whisper.Services.Validation.Interfaces;
using Whisper.Services.Factories.Interfaces;
using Whisper.DTOs.Response.Auth;
using Whisper.Services.Services.Interfaces;

namespace Whisper.Services.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailValidation _emailValidation;
        private readonly IPasswordValidation _passwordValidation;
        private readonly IAuthService _authService;
        private readonly IUserFactory _userFactory;

        public UserService(
            IUserRepository userRepository,
            IPasswordValidation passwordValidation,
            IEmailValidation emailValidation,
            IAuthService authService,
            IUserFactory userFactory)
        {
            _userRepository = userRepository;
            _passwordValidation = passwordValidation;
            _emailValidation = emailValidation;
            _authService = authService;
            _userFactory = userFactory;
        }

        public async Task<ApiResponse<UserSessionResponseDto>> Register(UserRegisterRequestDTO requestUser)
        {
            if (!_emailValidation.IsEmailValid(requestUser.Email))
            {
                return ApiResponse<UserSessionResponseDto>.Failure(AuthMessages.InvalidEmail, AuthCodes.InvalidEmail);
            }

            if (!_passwordValidation.IsStrong(requestUser.Password))
            {
                return ApiResponse<UserSessionResponseDto>.Failure(AuthMessages.WeakPassword, AuthCodes.WeakPassword);
            }

            (bool usernameExists, bool emailExists) = await _userRepository.CheckUserExistenceAsync(requestUser.Username, requestUser.Email);
            switch (true)
            {
                case true when usernameExists && emailExists:
                    return ApiResponse<UserSessionResponseDto>.Failure(AuthMessages.UsernameAndEmailExists, AuthCodes.UsernameAndEmailExists);
                case true when usernameExists:
                    return ApiResponse<UserSessionResponseDto>.Failure(AuthMessages.UsernameExists, AuthCodes.UsernameExists);
                case true when emailExists:
                    return ApiResponse<UserSessionResponseDto>.Failure(AuthMessages.EmailExists, AuthCodes.EmailExists);
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(requestUser.Password);
            (User user, UserCredentials credentials) = _userFactory.Map(requestUser, passwordHash);

            using var transaction = await _userRepository.BeginTransactionAsync();

            bool isRegistrationSuccess = await _userRepository.AddUserAsync(user);
            if (!isRegistrationSuccess)
            {
                await transaction.RollbackAsync();
                return ApiResponse<UserSessionResponseDto>.Failure(AuthMessages.RegistrationFailed, AuthCodes.RegistrationFailed);
            }

            bool isCredentialsSuccess = await _userRepository.SaveUserCredentialsAsync(credentials);
            if (!isCredentialsSuccess)
            {
                await transaction.RollbackAsync();
                return ApiResponse<UserSessionResponseDto>.Failure(AuthMessages.CredentialsSaveFailed, AuthCodes.CredentialsSaveFailed);
            }

            await transaction.CommitAsync();

            AuthResponseDto tokens = await _authService.GenerateAndAttachTokens(user);

            UserSessionResponseDto session = _userFactory.Map(tokens, user);

            return ApiResponse<UserSessionResponseDto>.Success(session, AuthMessages.UserRegistered);
        }

        public async Task<ApiResponse<UserSessionResponseDto>> Login(UserLoginRequestDTO requestUser)
        {
            User? user = await _userRepository.GetUserWithCredentialsByIdentifierAsync(requestUser.Username);

            if (user == null || user.Credentials == null)
            {
                return ApiResponse<UserSessionResponseDto>.Failure(AuthMessages.UserNotFound, AuthCodes.UserNotFound);
            }

            if (!BCrypt.Net.BCrypt.Verify(requestUser.Password, user.Credentials.PasswordHash))
            {
                return ApiResponse<UserSessionResponseDto>.Failure(AuthMessages.InvalidCredentials, AuthCodes.InvalidCredentials);
            }

            AuthResponseDto tokens = await _authService.GenerateAndAttachTokens(user);

            UserSessionResponseDto session = _userFactory.Map(tokens, user);

            return ApiResponse<UserSessionResponseDto>.Success(session, AuthMessages.UserLogged);
        }

        public async Task<ApiResponse<List<UserBasicDto>>> FindUsersByUsername(string query)
        {
            var users = await _userRepository.FindUsersByUsernameAsync(query);

            var userDtos = _userFactory.Map(users);

            return ApiResponse<List<UserBasicDto>>.Success(userDtos);
        }
    }
}
