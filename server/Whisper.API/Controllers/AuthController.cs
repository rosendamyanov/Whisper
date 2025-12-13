using Microsoft.AspNetCore.Mvc;
using Whisper.Authentication.Services.Interfaces;
using Whisper.Common.Response;
using Whisper.DTOs.Request.Auth;
using Whisper.DTOs.Response.Auth;

namespace Whisper.API.Controllers
{
    /// <summary>
    /// Handles user authentication operations including registration, login, token refresh, and logout
    /// </summary>
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Refreshes an expired access token using a valid refresh token
        /// </summary>
        /// <param name="refresh">Optional refresh token details (can also use cookies)</param>
        /// <returns>New JWT access token and refresh token</returns>
        /// <response code="200">Tokens successfully refreshed</response>
        /// <response code="401">Refresh failed due to invalid or expired refresh token</response>
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RefreshToken(RefreshRequestDTO? refresh)
        {
            ApiResponse<AuthResponseDto> response = await _authService.RefreshToken(refresh);
            return response.IsSuccess ? Ok(response) : Unauthorized(response);
        }
    }
}