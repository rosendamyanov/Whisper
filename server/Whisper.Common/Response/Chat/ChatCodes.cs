namespace Whisper.Common.Response.Chat
{
    public static class ChatCodes
    {
        public const string InvalidDM = "INVALID_DM";
        public const string UsersNotFound = "USERS_NOT_FOUND";
        public const string GroupNameRequired = "GROUP_NAME_REQUIRED";
        public const string InsufficientUsers = "INSUFFICIENT_USERS";
        public const string NotChatMember = "NOT_CHAT_MEMBER";
        public const string EmptyMessage = "EMPTY_MESSAGE";

        public const string MessageNotFound = "MESSAGE_NOT_FOUND";
        public const string UnauthorizedAction = "UNAUTHORIZED_ACTION";
        public const string FileUploadFailed = "FILE_UPLOAD_FAILED";
    }
}
