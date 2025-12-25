using Whisper.Common.Response;
using Whisper.Common.Response.Message;
using Whisper.Data.Repositories.Interfaces;
using Whisper.DTOs.Internal;
using Whisper.DTOs.Request.Message;
using Whisper.DTOs.Response.Message;
using Whisper.Services.Factories.Interfaces;
using Whisper.Services.Services.Interfaces;

namespace Whisper.Services.Services
{
    public class MessageService : IMessageService
    {
        private readonly IMessageRepository _messageRepository;
        private readonly ILocalFileStorageService _localFileStorageService;
        private readonly IMessageFactory _messageFactory;
        private readonly IChatNotificationService _chatNotificationService;

        public MessageService(
            IMessageRepository messageRepository,
            ILocalFileStorageService localFileStorageService,
            IMessageFactory messageFactory,
            IChatNotificationService chatNotificationService)
        {
            _messageRepository = messageRepository;
            _localFileStorageService = localFileStorageService;
            _messageFactory = messageFactory;
            _chatNotificationService = chatNotificationService;
        }

        public async Task<ApiResponse<MessageResponseDto>> SendMessageAsync(Guid userId, SendMessageRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Content) && (request.Files == null || !request.Files.Any()))
            {
                return ApiResponse<MessageResponseDto>.Failure(MessageMessages.EmptyMessage, MessageCodes.EmptyMessage);
            }

            List<FileSaveResult>? attachments = null;

            if (request.Files?.Any() == true)
            {
                attachments = new List<FileSaveResult>();
                foreach (var file in request.Files)
                {
                    using var stream = file.OpenReadStream();
                    var attachmentInfo = await _localFileStorageService.SaveFileAsync(
                        stream,
                        file.FileName,
                        file.ContentType,
                        file.Length
                    );
                    attachments.Add(attachmentInfo);
                }
            }

            var message = _messageFactory.Map(userId, request, attachments);

            await _messageRepository.AddAsync(message);
            var isSaved = await _messageRepository.SaveChangesAsync();

            if (!isSaved)
            {
                if (attachments != null && attachments.Any())
                {
                    foreach (var att in attachments)
                    {
                        await _localFileStorageService.DeleteFileAsync(att.Url);
                    }
                }
                return ApiResponse<MessageResponseDto>.Failure(MessageMessages.SaveFailed, MessageCodes.DbSaveError);
            }

            var savedMessage = await _messageRepository.GetByIdAsync(message.Id);
            var messageDto = _messageFactory.Map(savedMessage!, userId);

            await _chatNotificationService.BroadcastMessageAsync(savedMessage!.ChatId, messageDto);
            return ApiResponse<MessageResponseDto>.Success(messageDto, MessageMessages.MessageSent);
        }

        public async Task<ApiResponse<ChatLoadResponseDto>> GetChatMessagesAsync(Guid userId, Guid chatId, int limit, DateTime? before)
        {
            var chatMessages = await _messageRepository.GetChatHistoryAsync(chatId, limit, before);
            var unreadCount = await _messageRepository.GetUnreadCountAsync(chatId, userId);

            var messageDtos = _messageFactory.Map(chatMessages, userId, unreadCount);

            return ApiResponse<ChatLoadResponseDto>.Success(messageDtos);
        }

        public async Task<ApiResponse<MessageResponseDto>> EditMessageAsync(Guid userId, Guid messageId, string newContent)
        {
            var message = await _messageRepository.GetByIdAsync(messageId);

            if (message == null || message.IsDeleted || message.UserId != userId)
            {
                return ApiResponse<MessageResponseDto>.Failure(MessageMessages.MessageNotFoundOrAccessDenied, MessageCodes.NotFound);
            }

            message.Content = newContent;
            message.EditedAt = DateTime.UtcNow;

            await _messageRepository.UpdateAsync(message);
            var isSaved = await _messageRepository.SaveChangesAsync();

            var messageDto = _messageFactory.Map(message, userId);

            if (isSaved)
                await _chatNotificationService.BroadcastMessageEditedAsync(message.ChatId, messageDto);

            return isSaved
                ? ApiResponse<MessageResponseDto>.Success(messageDto, MessageMessages.MessageEdited)
                : ApiResponse<MessageResponseDto>.Failure(MessageMessages.EditFailed, MessageCodes.DbSaveError);
        }

        public async Task<ApiResponse<bool>> ReactToMessageAsync(Guid userId, Guid messageId, string emoji)
        {
            var exists = await _messageRepository.MessageExistsAsync(messageId);
            if (!exists)
            {
                return ApiResponse<bool>.Failure(MessageMessages.MessageNotFound, MessageCodes.NotFound);
            }

            var existingReaction = await _messageRepository.GetReactionAsync(userId, messageId, emoji);

            if (existingReaction != null)
            {
                _messageRepository.RemoveReaction(existingReaction);
                await _messageRepository.SaveChangesAsync();
                return ApiResponse<bool>.Success(true, MessageMessages.ReactionRemoved);
            }

            var newReaction = _messageFactory.Map(userId, messageId, emoji);

            await _messageRepository.AddReactionAsync(newReaction);
            var isSaved = await _messageRepository.SaveChangesAsync();

            if (!isSaved)
                return ApiResponse<bool>.Failure(MessageMessages.ReactionFailed, MessageCodes.DbError);

            var updatedMessage = await _messageRepository.GetByIdAsync(messageId);
            var dto = _messageFactory.Map(updatedMessage!, userId);

            await _chatNotificationService.BroadcastReactionChangedAsync(
                updatedMessage!.ChatId,
                messageId,
                dto.Reactions
            );

            return ApiResponse<bool>.Success(true, MessageMessages.ReactionAdded);
        }

        public async Task<ApiResponse<bool>> ReadMessagesAsync(Guid userId, Guid chatId, List<Guid> messageIds)
        {
            if (messageIds == null || !messageIds.Any())
            {
                return ApiResponse<bool>.Success(true);
            }

            await _messageRepository.MarkMessagesAsReadAsync(userId, messageIds);
            var isSaved = await _messageRepository.SaveChangesAsync();

            if (isSaved)
                await _chatNotificationService.BroadcastMessagesReadAsync(chatId, userId, messageIds);

            return ApiResponse<bool>.Success(true, MessageMessages.MessagesRead);
        }

        public async Task<ApiResponse<bool>> DeleteMessageAsync(Guid userId, Guid messageId)
        {
            var message = await _messageRepository.GetByIdAsync(messageId);

            if (message == null)
                return ApiResponse<bool>.Failure(MessageMessages.MessageNotFound, MessageCodes.NotFound);

            if (message.UserId != userId)
                return ApiResponse<bool>.Failure(MessageMessages.DeleteOwnMessagesOnly, MessageCodes.AccessDenied);


            if (message.Attachments != null && message.Attachments.Any())
            {
                foreach (var attachment in message.Attachments)
                {
                    await _localFileStorageService.DeleteFileAsync(attachment.Url);
                }
            }

            message.IsDeleted = true;
            message.DeletedAt = DateTime.UtcNow;

            await _messageRepository.UpdateAsync(message);
            var isSaved = await _messageRepository.SaveChangesAsync();

            if (isSaved)
            {
                await _chatNotificationService.BroadcastMessageDeletedAsync(message.ChatId, messageId);
                return ApiResponse<bool>.Success(true, MessageMessages.MessageDeleted);
            }

            return ApiResponse<bool>.Failure(MessageMessages.DeleteFailed, MessageCodes.DbError);
        }
    }
}