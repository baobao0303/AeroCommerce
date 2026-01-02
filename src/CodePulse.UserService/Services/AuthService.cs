using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using CodePulse.Shared.Contracts;
using CodePulse.UserService.Data;
using CodePulse.UserService.Domain.Entities;
using CodePulse.UserService.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CodePulse.UserService.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<UserDto?> GetUserByIdAsync(Guid id);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken);
    
    Task<bool> VerifyOtpAsync(VerifyOtpRequest request);
    Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<bool> ResetPasswordAsync(ResetPasswordRequest request);
}

public class AuthService : IAuthService
{
    private readonly UserDbContext _db;
    private readonly IConfiguration _config;

    public AuthService(UserDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        if (request.Password != request.ConfirmPassword)
            throw new ArgumentException("Mật khẩu xác nhận không khớp.");

        if (await _db.Users.AnyAsync(u => u.Email == request.Email))
            throw new UserAlreadyExistsException("email", request.Email);

        if (await _db.Users.AnyAsync(u => u.PhoneNumber == request.PhoneNumber))
            throw new UserAlreadyExistsException("phone", request.PhoneNumber);

        // Use domain factory method
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var user = User.Create(request.FirstName, request.LastName, request.Email, request.PhoneNumber, passwordHash);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // Phát sinh OTP (Giả lập gửi SMS/Email ở đây)
        var sp = new CodePulse.UserService.Data.StoredProcedures.UserStoredProcedures(_db);
        var otpCode = new Random().Next(100000, 999999).ToString();
        await sp.CreateOtpAsync(request.Email, otpCode, "Register", 5); // Chuyển sang lưu Email để gửi Email OTP

        // Gửi qua Email thực tế
        var subject = "Mã xác nhận đăng ký tài khoản (OTP) - CodePulse";
        var body = $"<p>Chào {request.FirstName},</p><p>Mã bảo mật OTP của bạn là: <strong>{otpCode}</strong>. Vui lòng nhập mã này để hoàn tất đăng ký. Mã có hiệu lực trong 5 phút.</p>";
        await SendEmailAsync(request.Email, subject, body);

        return GenerateAuthResponse(user);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var sp = new CodePulse.UserService.Data.StoredProcedures.UserStoredProcedures(_db);
        var user = await sp.GetUserByEmailOrPhoneAsync(request.Identifier)
            ?? throw new InvalidCredentialsException();

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new InvalidCredentialsException();

        return GenerateAuthResponse(user);
    }

    public async Task<bool> VerifyOtpAsync(VerifyOtpRequest request)
    {
        var sp = new CodePulse.UserService.Data.StoredProcedures.UserStoredProcedures(_db);
        try
        {
            await sp.VerifyAndConsumeOtpAsync(request.Receiver, request.Code, request.Type);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var sp = new CodePulse.UserService.Data.StoredProcedures.UserStoredProcedures(_db);
        var user = await sp.GetUserByEmailOrPhoneAsync(request.Identifier);
        if (user == null) return false;

        var otpCode = new Random().Next(100000, 999999).ToString();
        await sp.CreateOtpAsync(request.Identifier, otpCode, "ForgotPassword", 5);
        
        if (request.Identifier.Contains('@'))
        {
            var subject = "Yêu cầu khôi phục mật khẩu - CodePulse";
            var body = $"<p>Mã OTP khôi phục mật khẩu của bạn là: <strong>{otpCode}</strong>. Mã có hiệu lực trong 5 phút.</p>";
            await SendEmailAsync(request.Identifier, subject, body);
        }
        else 
        {
            Console.WriteLine($"[MOCK SMS] Gửi mã OTP khôi phục mật khẩu tới SĐT {request.Identifier}: {otpCode}");
        }
        
        return true;
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
    {
        if (request.NewPassword != request.ConfirmNewPassword)
            throw new ArgumentException("Mật khẩu xác nhận không khớp.");

        var sp = new CodePulse.UserService.Data.StoredProcedures.UserStoredProcedures(_db);
        try
        {
            // Verify OTP
            await sp.VerifyAndConsumeOtpAsync(request.Identifier, request.OtpCode, "ForgotPassword");

            // Update user password
            var user = await sp.GetUserByEmailOrPhoneAsync(request.Identifier);
            if (user != null)
            {
                user.UpdatePassword(BCrypt.Net.BCrypt.HashPassword(request.NewPassword));
                await _db.SaveChangesAsync();
                return true;
            }
        }
        catch { }
        return false;
    }

    public async Task<UserDto?> GetUserByIdAsync(Guid id)
    {
        var user = await _db.Users.FindAsync(id);
        return user is null ? null : MapToDto(user);
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        // TODO: Map bảng RefreshTokens vào UserDbContext rồi thực sự viết query vào database
        // Ở đây code tạm thời giả lập việc kiểm tra Refresh Token (MDC Plan).
        // 1. Tìm Refresh Token trong DB bằng EF Core hoặc dùng UserStoredProcedures.
        // var storedToken = await _db.RefreshTokens.SingleOrDefaultAsync(x => x.Token == refreshToken);
        // if (storedToken == null || storedToken.IsRevoked || storedToken.ExpiresAt < DateTime.UtcNow) throw ...

        // Giả sử lấy user thành công:
        var user = await _db.Users.FirstOrDefaultAsync(); // Mock
        if (user == null) throw new SecurityTokenException("User not found for this token.");

        // Tạo JWT và Refresh Token mới
        return GenerateAuthResponse(user);
    }

    private AuthResponse GenerateAuthResponse(User user)
    {
        var token = GenerateJwtToken(user);
        var expiresAt = DateTime.UtcNow.AddHours(24);
        return new AuthResponse(token, Guid.NewGuid().ToString(), expiresAt, MapToDto(user));
    }

    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _config["Jwt:Secret"] ?? throw new InvalidOperationException("JWT Secret not configured.")));

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("phoneNumber", user.PhoneNumber),
            new Claim("phoneVerified", user.PhoneNumberVerified.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static UserDto MapToDto(User user) =>
        new(user.Id, user.FirstName, user.LastName, user.Email, user.PhoneNumber, user.PhoneNumberVerified, user.Role, user.CreatedAt);

    private async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        try
        {
            var host = _config["Smtp:Host"];
            if (string.IsNullOrEmpty(host)) return; // Không có cấu hình thì bỏ qua

            var port = int.Parse(_config["Smtp:Port"] ?? "587");
            var username = _config["Smtp:Username"];
            var password = _config["Smtp:Password"];
            var enableSsl = bool.Parse(_config["Smtp:EnableSsl"] ?? "true");

            using var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = enableSsl
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(username!, "CodePulse Auth"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage);
            Console.WriteLine($"[SMTP] Đã gửi OTP thành công tới: {toEmail}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SMTP ERROR] Lỗi gửi email: {ex.Message}");
        }
    }
}
