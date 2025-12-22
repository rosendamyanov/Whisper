namespace Whisper.DTOs.Response.Message
{
    public class ReactionResponseDto
    {
        public string Emoji { get; set; }
        public int Count { get; set; }
        public bool IsReactedByMe { get; set; }
    }
}
