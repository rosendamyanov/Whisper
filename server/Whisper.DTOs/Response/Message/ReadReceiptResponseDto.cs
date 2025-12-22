namespace Whisper.DTOs.Response.Message
{
    public class ReadReceiptResponseDto
    {
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public string? AvatarUrl { get; set; }
        public DateTime ReadAt { get; set; }
    }
}
