namespace Whisper.DTOs.Response.Message
{
    public class AttachmentResponseDto
    {
        public string Url { get; set; }
        public string FileType { get; set; }
        public string FileName { get; set; }
        public long FileSize { get; set; }
    }
}
