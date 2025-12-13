using Whisper.Data.Models;
using Whisper.Data.Models.Authentication;
using Whisper.DTOs.Request.User;
using Whisper.DTOs.Response.Auth;
using Whisper.DTOs.Response.User;

namespace Whisper.Services.Factories.Interfaces
{
    public interface IUserFactory
    {
        public (User user, UserCredentials credentials) Map(UserRegisterRequestDTO request, string passwordHash);
        public UserSessionResponseDto Map(AuthResponseDto session, User user);
    }
}