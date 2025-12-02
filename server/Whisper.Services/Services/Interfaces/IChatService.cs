using Whisper.Common.Response;
using Whisper.DTOs.Request.Chat;
using Whisper.DTOs.Response.Chat;

namespace Whisper.Services.Services.Interfaces
{
    /// <summary>
    /// Provides business logic for chat operations including direct messages, group chats, and messaging
    /// </summary>
    public interface IChatService
    {
        /// <summary>
        /// Retrieves an existing direct message chat or creates a new one if it doesn't exist
        /// </summary>
        /// <param name="currentUserId">The ID of the user initiating the chat</param>
        /// <param name="friendId">The ID of the friend to chat with</param>
        /// <returns>ApiResponse containing chat details or error message</returns>
        /// <remarks>
        /// Implements "lazy chat creation" - DM chats are automatically created 
        /// when a user first messages a friend, similar to Discord's behavior.
        /// Validates that users aren't trying to message themselves.
        /// </remarks>
        Task<ApiResponse<ChatResponseDTO>> GetOrCreateDirectChatAsync(Guid currentUserId, Guid friendId);

        /// <summary>
        /// Creates a new group chat with multiple participants
        /// </summary>
        /// <param name="currentUserId">The ID of the user creating the group (becomes a participant)</param>
        /// <param name="request">Group chat details including name and list of participant IDs</param>
        /// <returns>ApiResponse containing created group chat details or error message</returns>
        /// <remarks>
        /// Validates that:
        /// - Group name is provided
        /// - At least 2 total users (creator + 1 other)
        /// - All specified users exist in the system
        /// 
        /// Automatically includes the creator as a participant.
        /// Removes duplicate user IDs before creating the group.
        /// </remarks>
        Task<ApiResponse<ChatResponseDTO>> CreateGroupChatAsync(Guid currentUserId, CreateGroupChatRequestDTO request);

        /// <summary>
        /// Sends a new message to a chat
        /// </summary>
        /// <param name="userId">The ID of the user sending the message</param>
        /// <param name="request">Message content and target chat ID</param>
        /// <returns>ApiResponse containing the created message with timestamp or error message</returns>
        /// <remarks>
        /// Validates that:
        /// - Message content is not empty
        /// - User is a member of the target chat
        /// 
        /// Message is timestamped server-side to prevent client time manipulation.
        /// </remarks>
        Task<ApiResponse<MessageResponseDTO>> SendMessageAsync(Guid userId, SendMessageRequestDTO request);

        /// <summary>
        /// Retrieves message history for a specific chat with pagination support
        /// </summary>
        /// <param name="chatId">The ID of the chat to retrieve messages from</param>
        /// <param name="userId">The ID of the user requesting messages (for authorization)</param>
        /// <param name="limit">Maximum number of messages to return (default: 50)</param>
        /// <param name="before">Optional timestamp to load messages before this time (for infinite scroll)</param>
        /// <returns>ApiResponse containing list of messages in chronological order or error message</returns>
        /// <remarks>
        /// Messages are returned in chronological order (oldest first) for display purposes.
        /// Use the 'before' parameter with the oldest message's timestamp to implement infinite scroll.
        /// Example: Load initial 50 messages, then use oldest message's timestamp to load next 50.
        /// 
        /// Validates user is a member of the chat before returning messages.
        /// </remarks>
        Task<ApiResponse<List<MessageResponseDTO>>> GetChatMessagesAsync(Guid chatId, Guid userId, int limit = 50, DateTime? before = null);

        /// <summary>
        /// Retrieves all chats for a user, sorted by most recent activity
        /// </summary>
        /// <param name="userId">The ID of the user whose chats to retrieve</param>
        /// <returns>ApiResponse containing list of chats with last message previews</returns>
        /// <remarks>
        /// Each chat includes:
        /// - Chat metadata (ID, name, type, participants)
        /// - Last message preview for display in chat list
        /// 
        /// Chats are ordered by most recent activity (last message timestamp).
        /// Both direct messages and group chats are included.
        /// </remarks>
        Task<ApiResponse<List<ChatResponseDTO>>> GetUserChatsAsync(Guid userId);
    }
}