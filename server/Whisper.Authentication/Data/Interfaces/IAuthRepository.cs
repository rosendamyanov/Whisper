using Microsoft.EntityFrameworkCore.Storage;
using Whisper.Data.Models;
using Whisper.Data.Models.Authentication;

namespace Whisper.Authentication.Data.Interfaces
{
    public interface IAuthRepository
    {
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task<User?> GetUserRefreshTokenAsync(string username);
        Task<RefreshToken?> GetRefreshTokenByIdAsync(Guid refreshTokenId, Guid? userId);
        Task<bool> SaveRevokedRefreshTokenAsync(RevokedToken revokedToken, RefreshToken refreshToken);
        Task<bool> SaveRefreshTokenAsync(RefreshToken refreshToken);
        Task<bool> SaveUserRefreshTokenAsync(User user);
    }
}
