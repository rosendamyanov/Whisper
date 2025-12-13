using Whisper.Data.Models;
using Whisper.DTOs.Response.Chat;
using Whisper.DTOs.Response.User;

namespace Whisper.Services.Factories.Interfaces
{
    public interface IChatFactory
    {
        Chat CreateDirectChatEntity(User user1, User user2);
        Chat CreateGroupChatEntity(string groupName, List<User> users);
        Message CreateMessageEntity(Guid chatId, Guid userId, string content);
        ChatResponseDTO MapToDto(Chat chat);
        List<ChatResponseDTO> MapToDto(List<Chat> chats);
        List<MessageResponseDTO> MapToDto(List<Message> messages);
        MessageResponseDTO MapToDto(Message message);
        UserBasicDto MapToDto(User user);
    }
}