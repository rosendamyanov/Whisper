using Whisper.Data.Models.Messages;
using Whisper.DTOs.Internal;
using Whisper.DTOs.Request.Message;
using Whisper.DTOs.Response.Message;
using Whisper.Services.Factories.Interfaces;

namespace Whisper.Services.Factories
{
    public class MessageFactory : IMessageFactory
    {

        public Message Map(Guid userId, SendMessageRequestDto request, List<FileSaveResult>? attachementsInfo)
        {
            var attachments = attachementsInfo?.Select(fileInfo => new MessageAttachment
            {
                Id = Guid.NewGuid(),
                MessageId = Guid.Empty, // Will be set after message creation
                Url = fileInfo.Url,
                FileName = fileInfo.FileName,
                FileType = fileInfo.FileType,
                FileSize = fileInfo.FileSize
            }).ToList() ?? new List<MessageAttachment>();

            return new Message
            {
                Id = Guid.NewGuid(),
                Content = request.Content,
                SentAt = DateTime.UtcNow,
                Type = (request.Files != null && request.Files.Any()) ? MessageType.File : MessageType.Text,
                UserId = userId,
                ChatId = request.ChatId,
                ReplyToId = request.ReplyToId,
                Attachments = attachments
            };
        }

        public MessageResponseDto Map(Message message, Guid currentUserId)
        {
            return new MessageResponseDto
            {
                Id = message.Id,
                Content = message.Content,
                SentAt = message.SentAt,
                IsEdited = message.EditedAt.HasValue,
                EditedAt = message.EditedAt,
                IsPinned = message.IsPinned,
                Type = message.Type.ToString(),

                SenderId = message.UserId,
                SenderName = message.User?.Username ?? "Unknown",
                SenderAvatarUrl = message.User?.ProfilePictureUrl, 
                IsMe = message.UserId == currentUserId, 

                ReplyToId = message.ReplyToId,
                ReplyToSenderName = message.ReplyTo?.User?.Username,
                ReplyToContent = message.ReplyTo?.Content,

                Attachments = message.Attachments.Select(att => new AttachmentResponseDto
                {
                    Url = att.Url,
                    FileName = att.FileName,
                    FileType = att.FileType,
                    FileSize = att.FileSize
                }).ToList(),

                Reactions = message.Reactions
                    .GroupBy(r => r.Content)
                    .Select(g => new ReactionResponseDto
                    {
                        Emoji = g.Key,
                        Count = g.Count(),
                        IsReactedByMe = g.Any(r => r.UserId == currentUserId)
                    }).ToList(),

                ReadBy = message.ReadReceipts
                    .Where(rr => rr.UserId != message.UserId)
                    .Select(rr => new ReadReceiptResponseDto
                    {
                        UserId = rr.UserId,
                        Username = rr.User?.Username ?? "Unknown",
                        AvatarUrl = rr.User?.ProfilePictureUrl,
                        ReadAt = rr.ReadAt
                    }).ToList()
            };
        }

        public ChatLoadResponseDto Map(List<Message> messages, Guid currentUserId, int unreadCount)
        {
            return new ChatLoadResponseDto
            {
                Messages = messages.Select(m => Map(m, currentUserId)).ToList(),
                UnreadCount = unreadCount
            };
        }

        public MessageReaction Map(Guid userId, Guid messageId, string emoji)
        {
            return new MessageReaction
            {
                Id = Guid.NewGuid(),
                MessageId = messageId,
                UserId = userId,
                Content = emoji,
                ReactedAt = DateTime.UtcNow
            };
        }

        public List<ReactionResponseDto> MapReactions(ICollection<MessageReaction> reactions, Guid currentUserId)
        {
            return reactions
                .GroupBy(r => r.Content)
                .Select(g => new ReactionResponseDto
                {
                    Emoji = g.Key,
                    Count = g.Count(),
                    IsReactedByMe = g.Any(r => r.UserId == currentUserId)
                }).ToList();
        }
    }
}
