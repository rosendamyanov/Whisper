using Whisper.Data.Models;
using Whisper.Data.Models.Authentication;
using Whisper.DTOs.Request.User;
using Whisper.DTOs.Response.Auth;
using Whisper.DTOs.Response.User;
using Whisper.Services.Factories.Interfaces;

namespace Whisper.Services.Factories
{
    public class UserFactory : IUserFactory
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

        public UserSessionResponseDto Map (AuthResponseDto session, User user)
        {
            UserSessionResponseDto responseDto = new UserSessionResponseDto
            {
                Session = session,
                User = new UserBasicDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Role = user.Role,
                }
            };
            return responseDto;
        }
    }
}
