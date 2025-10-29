using Identity.Application.Contracts;
using Identity.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.API.Controllers
{
    [ApiController]
    [Route("identity")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;


        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        //app.MapGet("/identity/health", () => Results.Ok(new { ok = true, svc = "identity" }));
        [HttpGet("health")]
        public async Task<IActionResult> Health()
        {
            return Ok(new { ok = true, svc = "identity" });
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.LoginAsync(request, cancellationToken);

            if (result == null)
                return Unauthorized(new { message = "Invalid email or password" });

            return Ok(result);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> Refresh()
        {
            try
            {
                // Lấy access token từ header Authorization
                string accessToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                if (string.IsNullOrEmpty(accessToken))
                    return Unauthorized(new { message = "Access token missing" });

                // Chỉ lấy refresh token từ cookie HttpOnly
                var result = await _authService.RefreshTokenAsync(accessToken);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }


        [AllowAnonymous]
        [HttpPost("social-login")]
        public async Task<IActionResult> SocialLogin([FromBody] SocialLoginRequest request)
        {
            var result = await _authService.SocialLoginAsync(request);
            if (result == null) return Unauthorized();
            return Ok(result);
        }
        //------------Register-------
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request);
            return Ok(result);
        }

        [HttpPost("verify-register-otp")]
        public async Task<IActionResult> VerifyRegisterOtp([FromBody] VerifyOtpRequest request)
        {
            var result = await _authService.VerifyOtpAndCreateUserAsync(request);
            return Ok(result);
        }

        [HttpPost("resend-otp")]
        public async Task<IActionResult> ResendOtp([FromBody] ResendOtpRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.EmailOrPhone))
                return BadRequest("Email hoặc Số điện thoại là bắt buộc.");

            var result = await _authService.ResendOtpAsync(request.EmailOrPhone);
            if (!result)
                return BadRequest("Không thể gửi lại mã OTP. Vui lòng thử lại sau.");

            return Ok(new { Message = "OTP mới đã được gửi." });
        }


        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request)
        {
            await _authService.LogoutAsync(request.RefreshToken);
            return Ok(new { message = "Đăng xuất thành công." });
        }

        [AllowAnonymous]
        [HttpPost("request-reset-password")]
        public async Task<IActionResult> RequestReset([FromBody] string emailOrPhone)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var result = await _authService.RequestResetPasswordAsync(emailOrPhone, baseUrl);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var result = await _authService.ResetPasswordAsync(request);
            return result ? Ok("Mật khẩu đã được reset") : BadRequest("Token không hợp lệ hoặc đã hết hạn");
        }
    }
}


