using Microsoft.AspNetCore.Mvc;
using Whisper.Authentication.DTOs.Request;
using Whisper.Authentication.DTOs.Response;
using Whisper.Authentication.Services.Interfaces;
using Whisper.Common.Response;

namespace Whisper.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegisterRequestDTO user)
        {
            ApiResponse<AuthResponseDto> response = await _authService.Register(user);

            if (response.IsSuccess)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLoginRequestDTO user)
        {
            ApiResponse<AuthResponseDto> response = await _authService.Login(user);
            if (response.IsSuccess)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken(RefreshRequestDTO? refresh)
        {
            ApiResponse<AuthResponseDto> response = await _authService.RefreshToken(refresh);
            if (response.IsSuccess)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout(LogoutRequestDTO? logoutRequestDto)
        {
            ApiResponse<string> response = await _authService.Logout(logoutRequestDto);
            if (response.IsSuccess)
            {
                return Ok(response);
            }
            return BadRequest(response);
        }
    }
}
