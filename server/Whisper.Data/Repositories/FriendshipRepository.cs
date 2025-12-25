using Microsoft.EntityFrameworkCore;
using Whisper.Data.Context;
using Whisper.Data.Models;
using Whisper.Data.Repositories.Interfaces;

namespace Whisper.Data.Repositories
{
    public class FriendshipRepository : IFriendshipRepository
    {
        private readonly ApplicationContext _context;

        public FriendshipRepository(ApplicationContext context)
        {
            _context = context;
        }


        // ====== Queries ======
        public async Task<Friendship?> GetFriendshipAsync(Guid userId, Guid friendId)
        {
            return await _context.Friendships
                .AsNoTracking()
                .Include(f => f.User)
                .Include(f => f.Friend)
                .FirstOrDefaultAsync(f => 
                        (f.UserId == userId && f.FriendId == friendId) || 
                        (f.UserId == friendId && f.FriendId == userId));
        }

        public async Task<List<Guid>> GetFriendsIdsAsync(Guid userId)
        {
            return await _context.Friendships
                .AsNoTracking()
                .Where(f => (f.UserId == userId || f.FriendId == userId) && f.IsAccepted)
                .Select(f => f.UserId == userId ? f.FriendId : f.UserId)
                .ToListAsync();
        }

        public async Task<List<Friendship>> GetFriendsAsync(Guid userId)
        {
            return await _context.Friendships
                .AsNoTracking()
                .Include(f => f.User)
                .Include(f => f.Friend)
                .Where(f => (f.UserId == userId || f.FriendId == userId) && f.IsAccepted)
                .ToListAsync();
        }

        public async Task<List<Friendship>> GetPendingRequestsAsync(Guid userId)
        {
            return await _context.Friendships
                .AsNoTracking()
                .Include(f => f.User)
                .Where(f => f.FriendId == userId && !f.IsAccepted)
                .ToListAsync();
        }

        public async Task<List<Friendship>> GetSentRequestsAsync(Guid userId)
        {
            return await _context.Friendships
                .AsNoTracking()
                .Include(f => f.Friend)
                .Where(f => f.UserId == userId && !f.IsAccepted)
                .ToListAsync();
        }

        public async Task<bool> FriendshipExistsAsync(Guid userId, Guid friendId)
        {
            return await _context.Friendships
                .AnyAsync(f =>
                    (f.UserId == userId && f.FriendId == friendId) ||
                    (f.UserId == friendId && f.FriendId == userId));
        }

        // ====== Actions ======
        public async Task<bool> SendFriendRequestAsync(Friendship friendship)
        {
            _context.Friendships.Add(friendship);
            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<bool> AcceptFriendRequestAsync(Guid friendshipId, Guid userId)
        {
            var rowsAffected = await _context.Friendships
                .Where(f => f.Id == friendshipId 
                             && f.FriendId == userId 
                             && !f.IsAccepted)
                .ExecuteUpdateAsync(f => f
                    .SetProperty(f => f.IsAccepted, true)
                    .SetProperty(f => f.AcceptedAt, DateTime.UtcNow)
                );
            return rowsAffected > 0;
        }
        public async Task<bool> DeclineFriendRequestAsync(Guid friendshipId, Guid userId)
        {
            var rowsAffected = await _context.Friendships
                .Where(f => f.Id == friendshipId 
                             && f.FriendId == userId 
                             && !f.IsAccepted)
                .ExecuteDeleteAsync();
            return rowsAffected > 0;
        }
        public async Task<bool> RemoveFriendAsync(Guid friendshipId, Guid userId)
        {
            var rowsAffected = await _context.Friendships
                .Where(f => f.Id == friendshipId
                            && (f.UserId == userId || f.FriendId == userId)
                            && f.IsAccepted)
                .ExecuteDeleteAsync();

            return rowsAffected > 0;
        }
    }
}
