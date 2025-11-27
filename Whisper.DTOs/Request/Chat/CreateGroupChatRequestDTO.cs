namespace Whisper.DTOs.Request.Chat
{
    public class CreateGroupChatRequestDTO
    {
        public string GroupName { get; set; }
        public List<Guid> UserIds { get; set; }
    }
}
