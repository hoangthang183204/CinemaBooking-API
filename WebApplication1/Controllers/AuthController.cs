using System.Security.Claims;
using CinemaBooking.Application.Common;
using CinemaBooking.Application.DTO.Auth;
using CinemaBooking.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaBooking.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;

        public AuthController(IAuthService auth) => _auth = auth;

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var result = await _auth.RegisterAsync(request);
                return Ok(ApiResponse<AuthResponse>.Ok(result, "Đăng ký thành công"));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ApiResponse<string>.Fail(ex.Message));
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _auth.LoginAsync(request);
            if (result == null)
                return Unauthorized(ApiResponse<string>.Fail("Email hoặc mật khẩu không đúng"));

            return Ok(ApiResponse<AuthResponse>.Ok(result, "Đăng nhập thành công"));
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> Me()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
                return Unauthorized(ApiResponse<string>.Fail("Token không hợp lệ"));

            var user = await _auth.GetCurrentUserAsync(userId);
            if (user == null)
                return NotFound(ApiResponse<string>.Fail("Không tìm thấy user"));

            return Ok(ApiResponse<AuthResponse>.Ok(user));
        }
    }
}