using CodePulse.Shared.Contracts;
using CodePulse.UserService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodePulse.UserService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthenticationController(IAuthService authService) => _authService = authService;

    /// <summary>Register a new user</summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var result = await _authService.RegisterAsync(request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    /// <summary>Login and receive JWT token</summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var result = await _authService.LoginAsync(request);
            SetTokensInsideCookie(result.Token, result.RefreshToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    /// <summary>Verify OTP</summary>
    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
    {
        var success = await _authService.VerifyOtpAsync(request);
        if (!success)
            return BadRequest(new { message = "Mã OTP không hợp lệ hoặc đã hết hạn." });
        return Ok(new { message = "Xác thực thành công." });
    }

    /// <summary>Forgot Password - Request OTP</summary>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var success = await _authService.ForgotPasswordAsync(request);
        if (!success)
            return NotFound(new { message = "Không tìm thấy người dùng với thông tin này." });
        return Ok(new { message = "Mã OTP đã được gửi." });
    }

    /// <summary>Reset Password</summary>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var success = await _authService.ResetPasswordAsync(request);
        if (!success)
            return BadRequest(new { message = "Đổi mật khẩu thất bại. OTP sai hoặc mật khẩu không khớp." });
        return Ok(new { message = "Đổi mật khẩu thành công." });
    }

    /// <summary>Get current authenticated user profile</summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserDto>> Me()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (!Guid.TryParse(userId, out var id))
            return Unauthorized();

        var user = await _authService.GetUserByIdAsync(id);
        return user is null ? NotFound() : Ok(user);
    }

    /// <summary>Refresh Access Token silently via HttpOnly Cookie</summary>
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> RefreshToken()
    {
        // 1. Lọc lấy Refresh Token từ Cookie (do trình duyệt tự đính kèm ở chế độ silent)
        var refreshToken = Request.Cookies["refreshToken"];
        
        if (string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized(new { message = "Không tìm thấy token gia hạn. Vui lòng đăng nhập lại." });
        }

        try
        {
            // 2. AuthService kiểm tra token trong DB
            var result = await _authService.RefreshTokenAsync(refreshToken);

            // 3. Set lại cặp token mới vào cookie
            SetTokensInsideCookie(result.Token, result.RefreshToken);

            return Ok(result); 
        }
        catch (Exception ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    // Helper: Gắn JWT và Refresh Token vào HttpOnly Cookie để bảo mật
    private void SetTokensInsideCookie(string accessToken, string refreshToken)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true, // Set to true for HTTPS
            SameSite = SameSiteMode.Strict, // Chống CSRF
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        };

        Response.Cookies.Append("accessToken", accessToken, cookieOptions);
        Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
    }
}
