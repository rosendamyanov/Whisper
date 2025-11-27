using Whisper.Common.Response;
using Whisper.Common.Response.Chat;
using Whisper.Data.Repositories.Interfaces;
using Whisper.DTOs.Request.Chat;
using Whisper.DTOs.Response.Chat;
using Whisper.Services.Factories.Interfaces;
using Whisper.Services.Services.Interfaces;

namespace Whisper.Services
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepository;
        private readonly IChatFactory _chatFactory;

        public ChatService(IChatRepository chatRepository, IChatFactory chatFactory)
        {
            _chatRepository = chatRepository;
            _chatFactory = chatFactory;
        }

        public async Task<ApiResponse<ChatResponseDTO>> GetOrCreateDirectChatAsync(Guid currentUserId, Guid friendId)
        {
            if (currentUserId == friendId)
                return ApiResponse<ChatResponseDTO>.Failure(ChatMessages.CannotMessageSelf, ChatCodes.InvalidDM);

            var existingChat = await _chatRepository.GetDirectMessageChatAsync(currentUserId, friendId);
            if (existingChat != null)
                return ApiResponse<ChatResponseDTO>.Success(_chatFactory.MapToDto(existingChat));

            var user1 = await _chatRepository.GetUserByIdAsync(currentUserId);
            var user2 = await _chatRepository.GetUserByIdAsync(friendId);

            if (user1 == null || user2 == null)
                return ApiResponse<ChatResponseDTO>.Failure(ChatMessages.UsersNotFound, ChatCodes.UsersNotFound);

            var newChat = _chatFactory.CreateDirectChatEntity(user1, user2);
            await _chatRepository.CreateChatAsync(newChat);


            return ApiResponse<ChatResponseDTO>.Success(_chatFactory.MapToDto(newChat));
        }

        public async Task<ApiResponse<ChatResponseDTO>> CreateGroupChatAsync(Guid currentUser, CreateGroupChatRequestDTO request)
        {
            if (request.GroupName == null)
                return ApiResponse<ChatResponseDTO>.Failure(ChatMessages.GroupNameRequired, ChatCodes.GroupNameRequired);
            if (request.UserIds == null || request.UserIds.Count < 1)
                return ApiResponse<ChatResponseDTO>.Failure(ChatMessages.InsufficientUsers, ChatCodes.InsufficientUsers);

            var allUsersIds = new List<Guid>(request.UserIds) { currentUser };
            allUsersIds = allUsersIds.Distinct().ToList();

            var users = await _chatRepository.GetUsersByIdsAsync(allUsersIds);
            if (users == null || users.Count != allUsersIds.Count)
                return ApiResponse<ChatResponseDTO>.Failure(ChatMessages.UsersNotFound, ChatCodes.UsersNotFound);

            var groupChat = _chatFactory.CreateGroupChatEntity(request.GroupName, users);

            await _chatRepository.CreateChatAsync(groupChat);

            return ApiResponse<ChatResponseDTO>.Success(_chatFactory.MapToDto(groupChat));
        }

        public async Task<ApiResponse<MessageResponseDTO>> SendMessageAsync(Guid userId, SendMessageRequestDTO request)
        {
            if (string.IsNullOrEmpty(request.Content))
                return ApiResponse<MessageResponseDTO>.Failure(ChatMessages.EmptyMessage, ChatCodes.EmptyMessage);

            bool isInChat = await _chatRepository.IsUserInChatAsync(request.ChatId, userId);
            if (!isInChat)
                return ApiResponse<MessageResponseDTO>.Failure(ChatMessages.NotChatMember, ChatCodes.NotChatMember);

            var message = _chatFactory.CreateMessageEntity(request.ChatId, userId, request.Content);

            var savedMessage = await _chatRepository.CreateMessageAsync(message);

            return ApiResponse<MessageResponseDTO>.Success(_chatFactory.MapToDto(savedMessage));
        }

        public async Task<ApiResponse<List<MessageResponseDTO>>> GetChatMessagesAsync(Guid chatId, Guid userId, int limit = 50, DateTime? before = null)
        {
            bool isInChat = await _chatRepository.IsUserInChatAsync(chatId, userId);
            if (!isInChat)
                return ApiResponse<List<MessageResponseDTO>>.Failure(ChatMessages.NotChatMember, ChatCodes.NotChatMember);

            var messages = await _chatRepository.GetChatMessagesAsync(chatId, limit, before);

            return ApiResponse<List<MessageResponseDTO>>.Success(_chatFactory.MapToDto(messages));
        }

        public async Task<ApiResponse<List<ChatResponseDTO>>> GetUserChatsAsync(Guid userId)
        {
            var chats = await _chatRepository.GetUserChatsAsync(userId);

            return ApiResponse<List<ChatResponseDTO>>.Success(_chatFactory.MapToDto(chats));
        }
    }
}
