using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Whisper.Common.Response;
using Whisper.DTOs.Request.Chat;
using Whisper.DTOs.Response.Chat;
using Whisper.Services;

namespace Whisper.API.Controllers
{
    [Route("api/chat")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly ChatService _chatService;
        public ChatController(ChatService chatService)
        {
            _chatService = chatService;
        }

        /// <summary>
        /// Get or create a direct message chat with a friend
        /// </summary>
        /// <param name="friendId">Friend's user ID</param>
        /// <returns>Chat metadata</returns>
        [HttpGet("dm/{friendId}")]
        public async Task<IActionResult> GetOrCreateDirectChat(Guid friendId)
        {
            var result = await _chatService.GetOrCreateDirectChatAsync(GetUserId(), friendId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Create a group chat with multiple users
        /// </summary>
        /// <param name="request">Group name and user IDs</param>
        /// <returns>Created group chat</returns>
        [HttpPost("group")]
        public async Task<IActionResult> CreateGroupChat([FromBody] CreateGroupChatRequestDTO request)
        {
            var result = await _chatService.CreateGroupChatAsync(GetUserId(), request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Get all chats for the current user
        /// </summary>
        /// <returns>List of chats with last message preview</returns>
        [HttpGet("my-chats")]
        public async Task<IActionResult> GetMyChats()
        {
            var result = await _chatService.GetUserChatsAsync(GetUserId());
            return Ok(result);
        }

        /// <summary>
        /// Get message history for a specific chat
        /// </summary>
        /// <param name="chatId">Chat ID</param>
        /// <param name="limit">Number of messages to load (default: 50)</param>
        /// <param name="before">Load messages before this timestamp (for pagination)</param>
        /// <returns>List of messages</returns>
        [HttpGet("{chatId}/messages")]
        public async Task<IActionResult> GetChatMessages(
            Guid chatId,
            [FromQuery] int limit = 50,
            [FromQuery] DateTime? before = null)
        {
            var result = await _chatService.GetChatMessagesAsync(chatId, GetUserId(), limit, before);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Send a message to a chat (also available via SignalR)
        /// </summary>
        /// <param name="request">Chat ID and message content</param>
        /// <returns>Created message</returns>
        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequestDTO request)
        {
            var result = await _chatService.SendMessageAsync(GetUserId(), request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim!);
        }

    }
}
