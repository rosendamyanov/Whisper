namespace Whisper.Common.Response.Message
{
    public static class MessageMessages
    {
        // Success
        public const string MessageSent = "Message sent successfully.";
        public const string MessageEdited = "Message edited successfully.";
        public const string MessageDeleted = "Message deleted successfully.";
        public const string ReactionAdded = "Reaction added.";
        public const string ReactionRemoved = "Reaction removed.";
        public const string MessagesRead = "Messages marked as read.";

        // Errors
        public const string EmptyMessage = "Message cannot be empty.";
        public const string MessageNotFound = "Message not found.";
        public const string MessageNotFoundOrAccessDenied = "Message not found or access denied.";
        public const string DeleteOwnMessagesOnly = "You can only delete your own messages.";
        public const string SaveFailed = "Failed to save message to database.";
        public const string EditFailed = "Failed to update message.";
        public const string DeleteFailed = "Failed to delete message.";
        public const string ReactionFailed = "Failed to save reaction.";
    }
}
