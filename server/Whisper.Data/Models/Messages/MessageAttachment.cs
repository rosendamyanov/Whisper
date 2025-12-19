namespace Whisper.Data.Models.Messages
{
    public class MessageAttachment
    {
        public Guid Id { get; set; }
        public string Url { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public long FileSize { get; set; }
        public Guid MessageId { get; set; }
        public Message Message { get; set; }
    }
}
