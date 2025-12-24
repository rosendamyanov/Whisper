namespace Whisper.Services.Services.Interfaces
{
    public interface IPresenceTracker
    {
        Task<bool> UserConnected(Guid userId, string connectionId);
        Task<bool> UserDisconnected(Guid userId, string connectionId);
    }
}