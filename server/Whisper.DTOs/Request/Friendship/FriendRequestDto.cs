

namespace Whisper.DTOs.Request.Friendship
{
    public class FriendRequestDto
    {
        public Guid FriendshipId { get; set; }
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public DateTime SentAt { get; set; }

    }
}
