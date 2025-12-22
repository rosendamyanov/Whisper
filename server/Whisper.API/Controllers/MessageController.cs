using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Whisper.Common.Response;
using Whisper.DTOs.Request.Message;
using Whisper.Services.Services.Interfaces;

namespace Whisper.API.Controllers
{
    [ApiController]
    [Route("api/message")]
    [Authorize]
    public class MessageController : ControllerBase
    {
        private readonly IMessageService _messageService;

        public MessageController(IMessageService messageService)
        {
            _messageService = messageService;
        }

        private Guid UserId => Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        [HttpGet("chat/{chatId}")]
        public async Task<IActionResult> GetChatMessages(Guid chatId, [FromQuery] int limit = 50, [FromQuery] DateTime? before = null)
        {
            var result = await _messageService.GetChatMessagesAsync(UserId, chatId, limit, before);
            return HandleResult(result);
        }

        [HttpPost]
        public async Task<IActionResult> SendMessage([FromForm] SendMessageRequestDto request)
        {
            var result = await _messageService.SendMessageAsync(UserId, request);
            return HandleResult(result);
        }

        [HttpPost("{id}/react")]
        public async Task<IActionResult> ReactToMessage(Guid id, [FromBody] ReactRequest request)
        {
            var result = await _messageService.ReactToMessageAsync(UserId, id, request.Emoji);
            return HandleResult(result);
        }

        [HttpPost("read")]
        public async Task<IActionResult> MarkMessagesAsRead([FromBody] List<Guid> messageIds)
        {
            var result = await _messageService.ReadMessagesAsync(UserId, messageIds);
            return HandleResult(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> EditMessage(Guid id, [FromBody] UpdateMessageRequest request)
        {
            var result = await _messageService.EditMessageAsync(UserId, id, request.Content);
            return HandleResult(result);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMessage(Guid id)
        {
            var result = await _messageService.DeleteMessageAsync(UserId, id);
            return HandleResult(result);
        }

        private IActionResult HandleResult<T>(ApiResponse<T> result)
        {
            if (result.IsSuccess) return Ok(result);

            return result.ErrorCode switch
            {
                "NOT_FOUND" => NotFound(result),
                "ACCESS_DENIED" => StatusCode(403, result),
                _ => BadRequest(result)
            };
        }
    }

    public record UpdateMessageRequest(string Content);
    public record ReactRequest(string Emoji);
}