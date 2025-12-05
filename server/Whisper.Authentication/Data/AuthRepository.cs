using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Whisper.Authentication.Data.Interfaces;
using Whisper.Data.Context;
using Whisper.Data.Models;
using Whisper.Data.Models.Authentication;

namespace Whisper.Authentication.Data
{
    public class AuthRepository : IAuthRepository
    {
        private readonly ApplicationContext _context;

        public AuthRepository(ApplicationContext context)
        {
            _context = context;
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }


        public async Task<User?> GetUserRefreshTokenAsync(string username)
        {
            return await _context.Users
                                 .Include(r => r.RefreshTokens)
                                 .Include(r => r.RevokedTokens)
                                 .FirstOrDefaultAsync(u => u.Username.Equals(username));
        }

        public async Task<RefreshToken?> GetRefreshTokenByIdAsync(Guid refreshTokenId, Guid? userId)
        {
            if (userId.HasValue)
            {
                return await _context.RefreshTokens
                    .FirstOrDefaultAsync(rt => rt.Id == refreshTokenId && rt.UserId == userId.Value);
            }

            return await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Id == refreshTokenId);
        }

        public async Task<bool> SaveRevokedRefreshTokenAsync(RevokedToken revokedToken, RefreshToken refreshToken)
        {
            _context.Update(refreshToken);
            _context.Add(revokedToken);

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> SaveRefreshTokenAsync(RefreshToken refreshToken)
        {
            _context.Add(refreshToken);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> SaveUserRefreshTokenAsync(User user)
        {
            _context.Users.Update(user);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
