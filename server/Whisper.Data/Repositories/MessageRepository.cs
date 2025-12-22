using Microsoft.EntityFrameworkCore;
using Whisper.Data.Context;
using Whisper.Data.Models.Messages;
using Whisper.Data.Repositories.Interfaces;

namespace Whisper.Data.Repositories
{
    /// <inheritdoc />
    public class MessageRepository : IMessageRepository
    {
        private readonly ApplicationContext _context;

        public MessageRepository(ApplicationContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task<Message?> GetByIdAsync(Guid messageId)
        {
            return await _context.Messages
                .Include(m => m.User)
                .Include(m => m.Attachments)
                .Include(m => m.ReplyTo).ThenInclude(r => r.User)
                .Include(m => m.Reactions)
                .Include(m => m.ReadReceipts).ThenInclude(r => r.User)
                .FirstOrDefaultAsync(m => m.Id == messageId);
        }

        public async Task<bool> MessageExistsAsync(Guid messageId)
        {
            return await _context.Messages
                .AnyAsync(m => m.Id == messageId && !m.IsDeleted);
        }

        /// <inheritdoc />
        public async Task AddAsync(Message message)
        {
            await _context.Messages.AddAsync(message);
        }

        /// <inheritdoc />
        public async Task UpdateAsync(Message message)
        {
            _context.Messages.Update(message);
            await Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task<List<Message>> GetChatHistoryAsync(Guid chatId, int limit, DateTime? before = null)
        {
            var query = _context.Messages
                .AsNoTracking()
                .Where(m => m.ChatId == chatId);

            if (before.HasValue)
            {
                query = query.Where(m => m.SentAt < before.Value);
            }

            return await query
                .Include(m => m.User)
                .Include(m => m.ReplyTo).ThenInclude(r => r.User)
                .Include(m => m.Reactions)
                .Include(m => m.ReadReceipts).ThenInclude(r => r.User)
                .Include(m => m.Attachments)
                .OrderByDescending(m => m.SentAt)
                .Take(limit)
                .AsSplitQuery()
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<int> GetUnreadCountAsync(Guid chatId, Guid userId)
        {
            return await _context.Messages
                .AsNoTracking()
                .Where(m => m.ChatId == chatId)
                .Where(m => m.UserId != userId)
                .Where(m => !m.ReadReceipts.Any(r => r.UserId == userId))
                .CountAsync();
        }

        /// <inheritdoc />
        public async Task MarkMessagesAsReadAsync(Guid userId, List<Guid> messageIds)
        {
            var validMessageIds = await _context.Messages
                .Where(m => messageIds.Contains(m.Id))
                .Where(m => m.UserId != userId)
                .Where(m => !m.ReadReceipts.Any(r => r.UserId == userId))
                .Select(m => m.Id)
                .ToListAsync();

            if (!validMessageIds.Any()) return;

            var newReceipts = validMessageIds.Select(msgId => new MessageReceipt
            {
                Id = Guid.NewGuid(),
                MessageId = msgId,
                UserId = userId,
                ReadAt = DateTime.UtcNow
            });

            await _context.MessageReceipts.AddRangeAsync(newReceipts);
        }

        /// <inheritdoc />
        public async Task AddReceiptAsync(MessageReceipt receipt)
        {
            await _context.MessageReceipts.AddAsync(receipt);
        }

        /// <inheritdoc />
        public async Task<bool> HasUserReadMessageAsync(Guid userId, Guid messageId)
        {
            return await _context.MessageReceipts
                .AnyAsync(r => r.MessageId == messageId && r.UserId == userId);
        }

        /// <inheritdoc />
        public async Task<MessageReaction?> GetReactionAsync(Guid userId, Guid messageId, string emoji)
        {
            return await _context.MessageReactions
                .FirstOrDefaultAsync(r => r.UserId == userId
                                       && r.MessageId == messageId
                                       && r.Content == emoji);
        }

        /// <inheritdoc />
        public async Task AddReactionAsync(MessageReaction reaction)
        {
            await _context.MessageReactions.AddAsync(reaction);
        }

        /// <inheritdoc />
        public void RemoveReaction(MessageReaction reaction)
        {
            _context.MessageReactions.Remove(reaction);
        }

        /// <inheritdoc />
        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}