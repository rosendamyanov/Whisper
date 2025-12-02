using Microsoft.EntityFrameworkCore;
using Whisper.Data.Context;
using Whisper.Data.Models;
using Whisper.Data.Repositories.Interfaces;

namespace Whisper.Data.Repositories
{
    public class ChatRepository : IChatRepository
    {
        private readonly ApplicationContext _context;

        public ChatRepository(ApplicationContext context)
        {
            _context = context;
        }

        // ====== DM Management ======

        public async Task<Chat?> GetDirectMessageChatAsync(Guid userId1, Guid userId2)
        {
            return await _context.Chats
                .AsNoTracking()
                .Include(c => c.Users)
                .Include(c => c.Messages.OrderByDescending(m => m.SentAt).Take(1))
                    .ThenInclude(m => m.User)
                .Where(c => !c.IsGroup
                        && c.Users.Any(u => u.Id == userId1)
                        && c.Users.Any(u => u.Id == userId2))
                .FirstOrDefaultAsync();
        }

        public async Task<Chat> CreateChatAsync(Chat chat)
        {
            _context.Chats.Add(chat);
            await _context.SaveChangesAsync();
            return chat;
        }

        // ====== Group Chat Management ======

        public async Task<bool> AddUserToChatAsync(Guid chatId, Guid userId)
        {
            Chat? chat = await _context.Chats
                .Include(c => c.Users)
                .FirstOrDefaultAsync(c => c.Id == chatId);

            if (chat == null)
                return false;

            User? user = await _context.Users.FindAsync(userId);
            if (user == null || chat.Users.Any(u => u.Id == userId))
                return false;

            chat.Users.Add(user);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> RemoveUserFromChatAsync(Guid chatId, Guid userId)
        {
            Chat? chat = await _context.Chats
                .Include(c => c.Users)
                .FirstOrDefaultAsync(c => c.Id == chatId);

            if (chat == null)
                return false;

            User? user = chat.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null)
                return false;

            chat.Users.Remove(user);
            return await _context.SaveChangesAsync() > 0;
        }

        // ====== Message Management ======

        public async Task<Message> CreateMessageAsync(Message message)
        {
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            await _context.Entry(message)
                .Reference(m => m.User)
                .LoadAsync();

            return message;
        }

        public async Task<List<Message>> GetChatMessagesAsync(Guid chatId, int limit, DateTime? before)
        {
            var query = _context.Messages
                .AsNoTracking()
                .Include(m => m.User)
                .Where(m => m.ChatId == chatId);

            if (before.HasValue)
            {
                query = query.Where(m => m.SentAt < before.Value);
            }

            return await query
                .OrderByDescending(m => m.SentAt)
                .Take(limit)
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }

        // ====== Chat queries ======

        public async Task<Chat?> GetChatByIdAsync(Guid chatId)
        {
            return await _context.Chats
                .AsNoTracking()
                .Include(c => c.Users)
                .Include(c => c.Messages.OrderByDescending(m => m.SentAt).Take(1))
                    .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(c => c.Id == chatId);
        }

        public async Task<List<Chat>> GetUserChatsAsync(Guid userId)
        {
            return await _context.Chats
                .AsNoTracking()
                .Include(c => c.Users)
                .Include(c => c.Messages.OrderByDescending(m => m.SentAt).Take(1))
                    .ThenInclude(m => m.User)
                .Where(c => c.Users.Any(u => u.Id == userId))
                .OrderByDescending(c => c.Messages.Any()
                    ? c.Messages.Max(m => m.SentAt)
                    : c.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> IsUserInChatAsync(Guid chatId, Guid userId)
        {
            return await _context.Chats
                .AsNoTracking()
                .AnyAsync(c => c.Id == chatId && c.Users.Any(u => u.Id == userId));
        }

        public async Task<User?> GetUserByIdAsync(Guid userId)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<List<User>> GetUsersByIdsAsync(List<Guid> userIds)
        {
            return await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync();
        }

        // ====== Utilities ======

        public async Task<bool> UpdateChatAsync(Chat chat)
        {
            _context.Chats.Update(chat);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
