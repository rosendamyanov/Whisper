namespace Whisper.DTOs.Response.Friendship
{
    public class FriendshipResponseDto
    {
        public Guid Id { get; set; }
        public FriendResponseDto Friend { get; set; }
        public bool IsAccepted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? AcceptedAt { get; set; }
    }
}
