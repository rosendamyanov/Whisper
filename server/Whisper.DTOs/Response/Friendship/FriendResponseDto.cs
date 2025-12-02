namespace Whisper.DTOs.Response.Friendship
{
    public class FriendResponseDto
    {
        public Guid Id { get; set; }
        public Guid FriendshipId { get; set; }
        public string Username { get; set; }
        public DateTime? FriendsSince { get; set; }
    }
}
