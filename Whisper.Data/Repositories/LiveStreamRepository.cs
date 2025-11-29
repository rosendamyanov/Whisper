using Microsoft.EntityFrameworkCore;
using Whisper.Data.Context;
using Whisper.Data.Models;
using Whisper.Data.Repositories.Interfaces;

namespace Whisper.Data.Repositories
{
    public class LiveStreamRepository : ILiveStreamRepository
    {
        private readonly ApplicationContext _context;

        public LiveStreamRepository(ApplicationContext context)
        {
            _context = context;
        }

        public async Task<LiveStream?> GetActiveStreamByChatIdAsync(Guid chatId)
        {
            var stream = await _context.Streams
                .AsNoTracking()
                .Include(s => s.HostUser)
                .FirstOrDefaultAsync(s => s.ChatId == chatId && s.IsLive);
            return stream;
        }
        public async Task<LiveStream?> GetByIdAsync(Guid streamId)
        {
            var stream = await _context.Streams
                .AsNoTracking()
                .Include(s => s.HostUser)
                .FirstOrDefaultAsync(s => s.Id == streamId);
            return stream;
        }
        public async Task<LiveStream> StartStreamAsync(LiveStream stream)
        {
            _context.Streams.Add(stream);
            await _context.SaveChangesAsync();

            await _context.Entry(stream)
                 .Reference(s => s.HostUser)
                 .LoadAsync();

            return stream;
        }
        public async Task<bool> EndStreamAsync(Guid streamId, Guid userId)
        {
            var rowsAffected = await _context.Streams
                 .Where(s => s.Id == streamId && s.HostUserId == userId && s.IsLive)
                 .ExecuteUpdateAsync(s => s
                     .SetProperty(ls => ls.IsLive, false)
                     .SetProperty(ls => ls.EndedAt, DateTime.UtcNow));
            return rowsAffected > 0;
        }
        public async Task<bool> IsUserStreamingAsync(Guid userId)
        {
            return await _context.Streams
                .AnyAsync(s => s.HostUserId == userId && s.IsLive);
        }
    }
}
