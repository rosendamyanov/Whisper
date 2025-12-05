using Whisper.Data.Models;
using Whisper.DTOs.Response.Chat;
using Whisper.DTOs.Response.User;
using Whisper.Services.Factories.Interfaces;

namespace Whisper.Services.Factories.ChatFactory
{
    public class ChatFactory : IChatFactory
    {
        // ========== ENTITY → DTO ==========

        public ChatResponseDTO MapToDto(Chat chat)
        {
            return new ChatResponseDTO
            {
                Id = chat.Id,
                IsGroup = chat.IsGroup,
                Name = chat.Name,
                CreatedAt = chat.CreatedAt,
                Participants = chat.Users.Select(MapToDto).ToList(),
                LastMessage = chat.Messages.Any()
                    ? MapToDto(chat.Messages.OrderByDescending(m => m.SentAt).First())
                    : null
            };
        }

        public MessageResponseDTO MapToDto(Message message)
        {
            return new MessageResponseDTO
            {
                Id = message.Id,
                Content = message.Content,
                SentAt = message.SentAt,
                UserId = message.UserId,
                Username = message.User?.Username ?? "Unknown",
                ChatId = message.ChatId
            };
        }

        public UserBasicDto MapToDto(User user)
        {
            return new UserBasicDto
            {
                Id = user.Id,
                Username = user.Username,
                Role = user.Role
            };
        }

        public List<ChatResponseDTO> MapToDto(List<Chat> chats)
        {
            return chats.Select(MapToDto).ToList();
        }

        public List<MessageResponseDTO> MapToDto(List<Message> messages)
        {
            return messages.Select(MapToDto).ToList();
        }

        // ========== DTO/DATA → ENTITY ==========

        public Chat CreateDirectChatEntity(User user1, User user2)
        {
            return new Chat
            {
                Id = Guid.NewGuid(),
                IsGroup = false,
                Name = null, // DMs don't have names
                CreatedAt = DateTime.UtcNow,
                Users = new List<User> { user1, user2 }
            };
        }

        public Chat CreateGroupChatEntity(string groupName, List<User> users)
        {
            return new Chat
            {
                Id = Guid.NewGuid(),
                IsGroup = true,
                Name = groupName,
                CreatedAt = DateTime.UtcNow,
                Users = users
            };
        }

        public Message CreateMessageEntity(Guid chatId, Guid userId, string content)
        {
            return new Message
            {
                Id = Guid.NewGuid(),
                ChatId = chatId,
                UserId = userId,
                Content = content,
                SentAt = DateTime.UtcNow
            };
        }
    }
}