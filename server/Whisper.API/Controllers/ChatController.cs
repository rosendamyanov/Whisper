using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Whisper.DTOs.Request.Chat;
using Whisper.Services.Services.Interfaces;
using Whisper.Common.Response;
using Whisper.DTOs.Response.Chat;

namespace Whisper.API.Controllers
{
    /// <summary>
    /// Manages chat operations including direct messages, group chats, and message history
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        /// <summary>
        /// Extracts the current user's ID from the JWT token claims
        /// </summary>
        /// <returns>User's unique identifier</returns>
        private Guid GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim!);
        }

        /// <summary>
        /// Gets an existing direct message chat or creates a new one with a friend
        /// </summary>
        /// <param name="friendId">The friend's user ID to chat with</param>
        /// <returns>Chat metadata including participants and last message preview</returns>
        /// <response code="200">Chat retrieved or created successfully</response>
        /// <response code="400">Invalid request (e.g., trying to message yourself or user not found)</response>
        /// <response code="401">User is not authenticated</response>
        [HttpGet("dm/{friendId}")]
        [ProducesResponseType(typeof(ApiResponse<ChatResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ChatResponseDTO>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetOrCreateDirectChat(Guid friendId)
        {
            var result = await _chatService.GetOrCreateDirectChatAsync(GetUserId(), friendId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Creates a new group chat with multiple participants
        /// </summary>
        /// <param name="request">Group chat details including name and participant IDs</param>
        /// <returns>Created group chat with all participants</returns>
        /// <response code="200">Group chat created successfully</response>
        /// <response code="400">Invalid request (e.g., missing group name, insufficient users, or users not found)</response>
        /// <response code="401">User is not authenticated</response>
        [HttpPost("group")]
        [ProducesResponseType(typeof(ApiResponse<ChatResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ChatResponseDTO>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CreateGroupChat([FromBody] CreateGroupChatRequestDTO request)
        {
            var result = await _chatService.CreateGroupChatAsync(GetUserId(), request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Retrieves all chats for the authenticated user
        /// </summary>
        /// <returns>List of chats with last message previews, sorted by most recent activity</returns>
        /// <response code="200">Chats retrieved successfully</response>
        /// <response code="401">User is not authenticated</response>
        [HttpGet("my-chats")]
        [ProducesResponseType(typeof(ApiResponse<List<ChatResponseDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMyChats()
        {
            var result = await _chatService.GetUserChatsAsync(GetUserId());
            return Ok(result);
        }

        /// <summary>
        /// Retrieves message history for a specific chat with pagination support
        /// </summary>
        /// <param name="chatId">The chat ID to retrieve messages from</param>
        /// <param name="limit">Maximum number of messages to return (default: 50, max: 100)</param>
        /// <param name="before">Optional timestamp to load messages before (for infinite scroll)</param>
        /// <returns>List of messages in chronological order</returns>
        /// <response code="200">Messages retrieved successfully</response>
        /// <response code="400">User is not a member of this chat</response>
        /// <response code="401">User is not authenticated</response>
        /// <remarks>
        /// For infinite scroll: Load initial 50 messages, then use the oldest message's timestamp 
        /// in the 'before' parameter to load older messages when user scrolls up.
        /// </remarks>
        [HttpGet("{chatId}/messages")]
        [ProducesResponseType(typeof(ApiResponse<List<MessageResponseDTO>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<MessageResponseDTO>>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetChatMessages(
            Guid chatId,
            [FromQuery] int limit = 50,
            [FromQuery] DateTime? before = null)
        {
            // Enforce limit constraints
            if (limit <= 0 || limit > 100)
                limit = 50;

            var result = await _chatService.GetChatMessagesAsync(chatId, GetUserId(), limit, before);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        /// <summary>
        /// Sends a new message to a chat (also available via SignalR for real-time delivery)
        /// </summary>
        /// <param name="request">Message content and target chat ID</param>
        /// <returns>The created message with timestamp and sender details</returns>
        /// <response code="200">Message sent successfully</response>
        /// <response code="400">Invalid request (e.g., empty message or user not in chat)</response>
        /// <response code="401">User is not authenticated</response>
        /// <remarks>
        /// This endpoint can be used for HTTP-based message sending, but for real-time chat 
        /// experience, use the SignalR ChatHub instead.
        /// </remarks>
        [HttpPost("send")]
        [ProducesResponseType(typeof(ApiResponse<MessageResponseDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<MessageResponseDTO>), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequestDTO request)
        {
            var result = await _chatService.SendMessageAsync(GetUserId(), request);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}