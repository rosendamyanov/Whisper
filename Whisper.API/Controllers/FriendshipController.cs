using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Whisper.Services.Services.Interfaces;

[Route("api/friends")]
[ApiController]
[Authorize]
public class FriendshipController : ControllerBase
{
    private readonly IFriendshipService _friendshipService;

    public FriendshipController(IFriendshipService friendshipService)
    {
        _friendshipService = friendshipService;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    [HttpGet]
    public async Task<IActionResult> GetFriends()
    {
        var userId = GetCurrentUserId();
        var result = await _friendshipService.GetFriendsAsync(userId);
        return Ok(result);
    }

    [HttpGet("{friendId}")]
    public async Task<IActionResult> GetFriendship(Guid friendId)
    {
        var userId = GetCurrentUserId();
        var result = await _friendshipService.GetFriendshipAsync(userId, friendId);
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }

    [HttpGet("requests/pending")]
    public async Task<IActionResult> GetPendingRequests()
    {
        var userId = GetCurrentUserId();
        var result = await _friendshipService.GetPendingRequestsAsync(userId);
        return Ok(result);
    }

    [HttpGet("requests/sent")]
    public async Task<IActionResult> GetSentRequests()
    {
        var userId = GetCurrentUserId();
        var result = await _friendshipService.GetSentRequestsAsync(userId);
        return Ok(result);
    }

    [HttpPost("request/{friendId}")]
    public async Task<IActionResult> SendFriendRequest(Guid friendId)
    {
        var userId = GetCurrentUserId();
        var result = await _friendshipService.SendFriendRequestAsync(userId, friendId);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("accept/{friendshipId}")]
    public async Task<IActionResult> AcceptFriendRequest(Guid friendshipId)
    {
        var userId = GetCurrentUserId();
        var result = await _friendshipService.AcceptFriendRequestAsync(friendshipId, userId);
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }

    [HttpPost("decline/{friendshipId}")]
    public async Task<IActionResult> DeclineFriendRequest(Guid friendshipId)
    {
        var userId = GetCurrentUserId();
        var result = await _friendshipService.DeclineFriendRequestAsync(friendshipId, userId);
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }

    [HttpDelete("{friendshipId}")]
    public async Task<IActionResult> RemoveFriend(Guid friendshipId)
    {
        var userId = GetCurrentUserId();
        var result = await _friendshipService.RemoveFriendAsync(friendshipId, userId);
        return result.IsSuccess ? Ok(result) : NotFound(result);
    }
}