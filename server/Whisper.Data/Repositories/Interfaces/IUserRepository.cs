using Microsoft.EntityFrameworkCore.Storage;
using Whisper.Data.Models;
using Whisper.Data.Models.Authentication;

namespace Whisper.Data.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task<bool> AddUserAsync(User user);
        Task<User?> GetUserByIdAsync(Guid userId);
        Task<(bool usernameExists, bool emailExists)> CheckUserExistenceAsync(string username, string email);
        Task<User?> GetUserByIdentifierAsync(string identifier);
        Task<User?> GetUserWithCredentialsByIdentifierAsync(string identifier);
        Task<bool> SaveUserCredentialsAsync(UserCredentials userCredentials);
        Task<List<User>> FindUsersByUsernameAsync(string query);
    }
}