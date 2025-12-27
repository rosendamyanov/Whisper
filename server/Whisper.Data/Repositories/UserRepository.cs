using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Whisper.Data.Context;
using Whisper.Data.Models;
using Whisper.Data.Models.Authentication;
using Whisper.Data.Repositories.Interfaces;

namespace Whisper.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationContext _context;

        public UserRepository(ApplicationContext context)
        {
            _context = context;
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }

        public async Task<bool> AddUserAsync(User user)
        {
            _context.Add(user);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<User?> GetUserByIdAsync(Guid userId)
        {
            return await _context.Users
                                 .Include(u => u.RefreshTokens)
                                 .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<(bool usernameExists, bool emailExists)> CheckUserExistenceAsync(string username, string email)
        {

            var matches = await _context.Users
               .IgnoreQueryFilters()
               .Where(u => u.Username == username || u.Email == email)
               .Select(u => new
               {
                   u.Username,
                   u.Email
               })
               .ToListAsync();

            return (
                usernameExists: matches.Any(u => u.Username == username),
                emailExists: matches.Any(u => u.Email == email)
                );
        }

        public async Task<User?> GetUserByIdentifierAsync(string identifier)
        {
            return await _context.Users
                                 .Include(u => u.RefreshTokens)
                                 .FirstOrDefaultAsync(u => u.Username == identifier || u.Email == identifier);
        }

        public async Task<bool> SaveUserCredentialsAsync(UserCredentials userCredentials)
        {
            _context.Add(userCredentials);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<User?> GetUserWithCredentialsByIdentifierAsync(string identifier)
        {
            return await _context.Users
                .Include(u => u.Credentials)
                .Include(u => u.RefreshTokens)
                .FirstOrDefaultAsync(u => u.Username == identifier || u.Email == identifier);
        }

        public async Task<List<User>> FindUsersByUsernameAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 3)
                return new List<User>();

            return await _context.Users
                .AsNoTracking()
                .Where(u => u.Username.Contains(query))
                .OrderBy(u => u.Username)
                .Take(10)
                .ToListAsync();
        }
     }
}
