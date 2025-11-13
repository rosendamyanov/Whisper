namespace Whisper.Common.Response.Authentication
{
    public static class ResponseCodes
    {
        public const string UsernameExists = "USERNAME_EXISTS";
        public const string UsernameNotFound = "USERNAME_NOT_FOUND";
        public const string UsernameAndEmailExists = "USERNAME_EMAIL_EXISTS";
        public const string UserNotFound = "USER_NOT_FOUND";
        public const string EmailExists = "EMAIL_EXISTS";
        public const string InvalidEmail = "EMAIL_INVALID";
        public const string RegistrationFailed = "REGISTRATION_FAILED";
        public const string InvalidCredentials = "INVALID_CREDENTIALS";
        public const string WeakPassword = "WEAK_PASSWORD";
        public const string RefreshNotFound = "REFRESH_TOKEN_NOT_FOUND";
        public const string RefreshIsRevokedOrExpired = "REFRESH_TOKEN_IS_REVOKED_OR_EXPIRED";
        public const string InvalidAccessToken = "INVALID_ACCESS_TOKEN";
        public const string RefreshRevokeFailed = "REFRESH_REVOKE_FAILED";
        public const string RefreshAddFailed = "REFRESH_ADD_FAILED";
        public const string CredentialsSaveFailed = "CREDENTIALS_SAVE_FAILED";
    }
}
