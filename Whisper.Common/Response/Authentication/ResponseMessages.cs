namespace Whisper.Common.Response.Authentication
{
    public static class ResponseMessages
    {
        //Success Messages
        public const string UserRegistered = "User registered successfully.";
        public const string UserLogged = "Successfully logged in.";
        public const string TokenRefreshed = "Refresh token successfully refreshed.";

        //Failure Messages
        public const string UsernameExists = "Username already exists.";
        public const string UsernameNotFound = "Username does not exists.";
        public const string UsernameAndEmailExists = "Username and email exists.";
        public const string UserNotFound = "User not found";
        public const string EmailExists = "Email already exists.";
        public const string InvalidEmail = "Email is invalid.";
        public const string RegistratoinFailed = "Registration failed, please try again later.";
        public const string InvalidCredentials = "Wrong username or password.";
        public const string WeakPassword = "Password must contain uppercase, lowercase, number, and special character.";
        public const string RefreshNotFound = "Refresh token not found.";
        public const string RefreshIsRevokedOrExpired = "Refresh token is revoked or expired.";
        public const string InvalidAccessToken = "Access token is invalid.";
        public const string RefreshRevokeFailed = "Failed to revoke refresh token. Please try again later.";
        public const string RefreshAddFailed = "Failed to add refresh token. Please try again later.";
    }
}