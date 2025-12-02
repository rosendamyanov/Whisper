using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Whisper.Services.Services.Interfaces;

namespace Whisper.API.Controllers
{
    [Route("api/stream")]
    [ApiController]
    [Authorize]
    public class LiveStreamController : ControllerBase
    {
        private readonly ILiveStreamService _liveStreamService;
        public LiveStreamController(ILiveStreamService liveStreamService)
        {
            _liveStreamService = liveStreamService;
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return Guid.Parse(userIdClaim!);
        }

        [HttpGet("active/{chatId}")]
        public async Task<IActionResult> GetActiveStream(Guid chatId)
        {
            var userId = GetCurrentUserId();
            var stream = await _liveStreamService.GetActiveStreamAsync(chatId, userId);
            return stream.IsSuccess ? Ok(stream) : BadRequest(stream);
        }

        [HttpPost("start/{chatId}")]
        public async Task<IActionResult> StartStream(Guid chatId)
        {
            var userId = GetCurrentUserId();
            var stream = await _liveStreamService.StartStreamAsync(chatId, userId);
            return stream.IsSuccess ? Ok(stream) : BadRequest(stream);
        }

        [HttpPost("end/{streamId}")]
        public async Task<IActionResult> EndStream(Guid streamId)
        {
            var userId = GetCurrentUserId();
            var result = await _liveStreamService.EndStreamAsync(streamId, userId);
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }
    }
}
