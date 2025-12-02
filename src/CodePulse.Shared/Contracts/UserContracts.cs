namespace CodePulse.Shared.Contracts;

// ── Auth ──────────────────────────────────────────
public record RegisterRequest(string FirstName, string LastName, string Email, string PhoneNumber, string Password, string ConfirmPassword);
public record LoginRequest(string Identifier, string Password); // Identifier có thể là Email hoặc Phone
public record AuthResponse(string Token, string RefreshToken, DateTime ExpiresAt, UserDto User);

public record VerifyOtpRequest(string Receiver, string Code, string Type);
public record ForgotPasswordRequest(string Identifier);
public record ResetPasswordRequest(string Identifier, string OtpCode, string NewPassword, string ConfirmNewPassword);

// ── User ──────────────────────────────────────────
public record UserDto(Guid Id, string FirstName, string LastName, string Email, string PhoneNumber, bool PhoneNumberVerified, string Role, DateTime CreatedAt);
