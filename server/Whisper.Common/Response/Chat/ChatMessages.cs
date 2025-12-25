namespace Whisper.Common.Response.Chat
{
    public static class ChatMessages
    {
        public const string CannotMessageSelf = "Cannot DM yourself.";
        public const string UsersNotFound = "One or more users were not found.";
        public const string GroupNameRequired = "Group name required!";
        public const string InsufficientUsers = "At least 2 users required.";
        public const string NotChatMember = "You are not in this chat.";
        public const string EmptyMessage = "Message cannot be empty.";

        public const string MessageNotFound = "Message not found.";
        public const string UnauthorizedEdit = "You can only edit your own messages.";
        public const string UnauthorizedDelete = "You can only delete your own messages.";
        public const string FileUploadFailed = "Failed to upload file.";
    }
}