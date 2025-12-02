public static class FriendshipMessages
{
    // Queries
    public const string FriendshipNotFound = "Friendship not found.";
    public const string NoFriendsFound = "No friends found.";
    public const string NoPendingRequestsFound = "No pending requests found.";
    public const string NoSentRequestsFound = "No sent requests found.";

    // Actions - Success
    public const string FriendRequestSent = "Friend request sent successfully.";
    public const string FriendRequestAccepted = "Friend request accepted successfully.";
    public const string FriendRequestDeclined = "Friend request declined successfully.";
    public const string FriendRemoved = "Friend removed successfully.";

    // Actions - Failure
    public const string CannotFriendSelf = "Cannot send friend request to oneself.";
    public const string FriendshipAlreadyExists = "Friendship or request already exists.";
    public const string SendFriendRequestFailed = "Failed to send friend request.";
    public const string AcceptFriendRequestFailed = "Failed to accept friend request.";
    public const string DeclineFriendRequestFailed = "Failed to decline friend request.";
    public const string RemoveFriendFailed = "Failed to remove friend.";
}