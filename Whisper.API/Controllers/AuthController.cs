using Microsoft.AspNetCore.Mvc;
using Whisper.Authentication.DTOs.Request;
using Whisper.Authentication.DTOs.Response;
using Whisper.Authentication.Services.Interfaces;
using Whisper.Common.Response;

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
        /// Registers a new user account
        /// </summary>
        /// <param name="user">Registration details including username, email, and password</param>
        /// <returns>JWT access token and refresh token on success</returns>
        /// <response code="200">User successfully registered and tokens returned</response>
        /// <response code="400">Registration failed due to validation errors or existing user</response>
        [HttpPost("register")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register(UserRegisterRequestDTO user)
        {
            ApiResponse<AuthResponseDto> response = await _authService.Register(user);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Authenticates a user and returns JWT tokens
        /// </summary>
        /// <param name="user">Login credentials (username/email and password)</param>
        /// <returns>JWT access token and refresh token on success</returns>
        /// <response code="200">Login successful, tokens returned</response>
        /// <response code="400">Login failed due to invalid credentials</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login(UserLoginRequestDTO user)
        {
            ApiResponse<AuthResponseDto> response = await _authService.Login(user);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Refreshes an expired access token using a valid refresh token
        /// </summary>
        /// <param name="refresh">Optional refresh token details (can also use cookies)</param>
        /// <returns>New JWT access token and refresh token</returns>
        /// <response code="200">Tokens successfully refreshed</response>
        /// <response code="400">Refresh failed due to invalid or expired refresh token</response>
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RefreshToken(RefreshRequestDTO? refresh)
        {
            ApiResponse<AuthResponseDto> response = await _authService.RefreshToken(refresh);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Logs out the current user by revoking their refresh token
        /// </summary>
        /// <param name="logoutRequestDto">Optional logout details (can also use cookies)</param>
        /// <returns>Success message on logout</returns>
        /// <response code="200">User successfully logged out</response>
        /// <response code="400">Logout failed due to missing or invalid tokens</response>
        [HttpPost("logout")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Logout(LogoutRequestDTO? logoutRequestDto)
        {
            ApiResponse<string> response = await _authService.Logout(logoutRequestDto);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }
    }
}