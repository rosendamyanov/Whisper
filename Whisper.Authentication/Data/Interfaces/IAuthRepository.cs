using Microsoft.EntityFrameworkCore.Storage;
using Whisper.Data.Models;
using Whisper.Data.Models.Authentication;

namespace Whisper.Authentication.Data.Interfaces
{
    public interface IAuthRepository
    {
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task<bool> AddUserAsync(User user);
        Task<(bool usernameExists, bool emailExists)> CheckUserExistenceAsync(string username, string email);
        Task<User?> GetUserByIdentifierAsync(string identifier);
        Task<User?> GetUserWithCredentialsByIdentifierAsync(string identifier);
        Task<User?> GetUserRefreshTokenAsync(string username);
        Task<RefreshToken?> GetRefreshTokenByIdAsync(Guid refreshTokenId, Guid? userId);
        Task<bool> SaveRevokedRefreshTokenAsync(RevokedToken revokedToken, RefreshToken refreshToken);
        Task<bool> SaveRefreshTokenAsync(RefreshToken refreshToken);
        Task<bool> SaveUserRefreshTokenAsync(User user);
        Task<bool> SaveUserCredentialsAsync(UserCredentials userCredentials);

    }
}
