using AeroCommerce.UserService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AeroCommerce.UserService.Data.StoredProcedures;

/// <summary>
/// Wrapper gọi Stored Procedures của UserService.
/// Dùng khi cần query phức tạp mà LINQ không đủ mạnh.
/// </summary>
public class UserStoredProcedures
{
    private readonly UserDbContext _db;

    public UserStoredProcedures(UserDbContext db) => _db = db;

    /// <summary>
    /// SP: Tìm user theo email (case-insensitive).
    /// </summary>
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _db.Users
            .FromSqlRaw("SELECT * FROM sp_get_user_by_email({0})", email.ToLowerInvariant())
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// SP: Lấy danh sách users active với phân trang.
    /// </summary>
    public async Task<List<User>> GetActiveUsersPagedAsync(int page, int pageSize)
    {
        var offset = (page - 1) * pageSize;
        return await _db.Users
            .FromSqlRaw("SELECT * FROM sp_get_active_users_paged({0}, {1})", offset, pageSize)
            .ToListAsync();
    }

    /// <summary>
    /// SP: Soft delete user (đánh dấu IsActive = false).
    /// </summary>
    public async Task<int> SoftDeleteUserAsync(Guid userId)
    {
        return await _db.Database
            .ExecuteSqlRawAsync("CALL sp_soft_delete_user({0})", userId);
    }

    /// <summary>
    /// SP: Tìm User bằng Email hoặc SĐT (Dùng cho luồng Login mới linh hoạt).
    /// </summary>
    public async Task<User?> GetUserByEmailOrPhoneAsync(string identifier)
    {
        return await _db.Users
            .FromSqlRaw("SELECT * FROM sp_get_user_by_email_or_phone({0})", identifier.ToLowerInvariant().Trim())
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// SP: Cấp mã OTP và vô hiệu hóa các mã cũ chưa dùng của Receiver (Phone/Email) này.
    /// </summary>
    public async Task<int> CreateOtpAsync(string receiver, string code, string type, int expiredInMinutes)
    {
        return await _db.Database
            .ExecuteSqlRawAsync("CALL sp_create_otp({0}, {1}, {2}, {3})", receiver, code, type, expiredInMinutes);
    }

    /// <summary>
    /// SP: Xác thực OTP và đánh dấu IsUsed. Update PhoneNumberVerified luôn nếu là Register.
    /// </summary>
    public async Task<int> VerifyAndConsumeOtpAsync(string receiver, string code, string type)
    {
        return await _db.Database
            .ExecuteSqlRawAsync("CALL sp_verify_and_consume_otp({0}, {1}, {2})", receiver, code, type);
    }
}
