using Whisper.Data.Models.Messages;

namespace Whisper.Data.Repositories.Interfaces
{
    /// <summary>
    /// Handles all database operations for Messages, including History, Attachments, Reactions, and Read Receipts.
    /// </summary>
    public interface IMessageRepository
    {
        // ----------------------------------------------------------------
        // 1. MESSAGES (CRUD + HISTORY)
        // ----------------------------------------------------------------

        /// <summary>
        /// Retrieves a single message by ID, including Sender, Attachments, Reply Context, Reactions, and Receipts.
        /// </summary>
        /// <param name="messageId">The GUID of the message.</param>
        /// <returns>The fully populated message entity or null if not found.</returns>
        Task<Message?> GetByIdAsync(Guid messageId);


        Task<bool> MessageExistsAsync(Guid messageId);

        /// <summary>
        /// Adds a new message to the database context. 
        /// Note: Call SaveChangesAsync() to persist.
        /// </summary>
        Task AddAsync(Message message);

        /// <summary>
        /// Marks a message entity as modified.
        /// </summary>
        Task UpdateAsync(Message message);

        /// <summary>
        /// Retrieves a paginated list of messages for a specific chat.
        /// Uses Cursor Pagination (SentAt) for stable infinite scrolling.
        /// </summary>
        /// <param name="chatId">The Chat ID.</param>
        /// <param name="limit">How many messages to fetch (e.g., 25).</param>
        /// <param name="before">
        /// If provided, fetches messages sent BEFORE this date. 
        /// Used for scrolling UP to load older history. 
        /// If null, fetches the newest messages.
        /// </param>
        /// <returns>A list of messages ordered chronologically (Oldest -> Newest) for the UI.</returns>
        Task<List<Message>> GetChatHistoryAsync(Guid chatId, int limit, DateTime? before = null);


        // ----------------------------------------------------------------
        // 2. READ RECEIPTS
        // ----------------------------------------------------------------

        /// <summary>
        /// Adds a single read receipt to the database.
        /// </summary>
        Task AddReceiptAsync(MessageReceipt receipt);

        /// <summary>
        /// Checks if a specific user has already marked a specific message as read.
        /// </summary>
        Task<bool> HasUserReadMessageAsync(Guid userId, Guid messageId);

        /// <summary>
        /// Marks a list of messages as read for a specific user.
        /// Internally filters out messages that are:
        /// 1. Sent by the user themselves (Self-read).
        /// 2. Already read by the user (Duplicates).
        /// </summary>
        /// <param name="userId">The user reading the messages.</param>
        /// <param name="messageIds">The list of Message IDs currently visible on screen.</param>
        Task MarkMessagesAsReadAsync(Guid userId, List<Guid> messageIds);

        /// <summary>
        /// Calculates the total number of unread messages for a user in a specific chat.
        /// </summary>
        Task<int> GetUnreadCountAsync(Guid chatId, Guid userId);


        // ----------------------------------------------------------------
        // 3. REACTIONS
        // ----------------------------------------------------------------

        /// <summary>
        /// Adds a reaction (e.g., "❤️") to a message.
        /// </summary>
        Task AddReactionAsync(MessageReaction reaction);

        /// <summary>
        /// Removes a specific reaction from a message.
        /// </summary>
        void RemoveReaction(MessageReaction reaction);

        /// <summary>
        /// Retrieves a specific reaction entity to check existence or ownership.
        /// </summary>
        Task<MessageReaction?> GetReactionAsync(Guid userId, Guid messageId, string emoji);


        // ----------------------------------------------------------------
        // 4. TRANSACTION
        // ----------------------------------------------------------------

        /// <summary>
        /// Commits all tracked changes (Adds, Updates, Deletes) to the database.
        /// </summary>
        /// <returns>True if any rows were affected.</returns>
        Task<bool> SaveChangesAsync();
    }
}