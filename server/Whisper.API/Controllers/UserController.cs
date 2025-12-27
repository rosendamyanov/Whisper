using Microsoft.AspNetCore.Mvc;
using Whisper.Authentication.Services.Interfaces;
using Whisper.Common.Response;
using Whisper.DTOs.Request.User;
using Whisper.DTOs.Response.User;
using Whisper.Services.Services.Interfaces;

namespace Whisper.API.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IAuthService _authService;

        public UserController(IUserService userService, IAuthService authService)
        {
            _userService = userService;
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
        [ProducesResponseType(typeof(ApiResponse<UserSessionResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<UserSessionResponseDto>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Register(UserRegisterRequestDTO user)
        {
            ApiResponse<UserSessionResponseDto> response = await _userService.Register(user);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Authenticates a user and returns JWT tokens
        /// </summary>
        /// <param name="user">Login credentials (username/email and password)</param>
        /// <returns>JWT access token and refresh token on success</returns>
        /// <response code="200">Login successful, tokens returned</response>
        /// <response code="401">Login failed due to invalid credentials</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(ApiResponse<UserSessionResponseDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<UserSessionResponseDto>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login(UserLoginRequestDTO user)
        {
            ApiResponse<UserSessionResponseDto> response = await _userService.Login(user);
            return response.IsSuccess ? Ok(response) : Unauthorized(response);
        }

        /// <summary>
        /// Logs out the current user by revoking their refresh token
        /// </summary>
        /// <param name="logoutRequestDto">Optional logout details (can also use cookies)</param>
        /// <returns>Success message on logout</returns>
        /// <response code="200">User successfully logged out</response>
        /// <response code="401">Logout failed due to missing or invalid tokens</response>
        [HttpPost("logout")]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Logout(LogoutRequestDTO? logoutRequestDto)
        {
            ApiResponse<string> response = await _authService.Logout(logoutRequestDto);
            return response.IsSuccess ? Ok(response) : Unauthorized(response);
        }

        [HttpGet("search")]
        public async Task<IActionResult> FindUsersByUsername([FromQuery] string query)
        {
            var response = await _userService.FindUsersByUsername(query);
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }
    }
}
