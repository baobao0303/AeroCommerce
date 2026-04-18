using AeroCommerce.UserService.Data;

namespace AeroCommerce.UserService.Domain.Entities;

/// <summary>
/// Represents an application user.
/// </summary>
public class User : IAuditableEntity
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public bool PhoneNumberVerified { get; private set; } = false;
    public string PasswordHash { get; private set; } = string.Empty;
    public string Role { get; private set; } = UserRole.User;
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }

    // ── EF Core requires a parameterless constructor ──
    private User() { }

    // ── Factory method — giữ business logic trong domain ──
    public static User Create(string firstName, string lastName, string email, string phoneNumber, string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
        ArgumentException.ThrowIfNullOrWhiteSpace(lastName);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(phoneNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        return new User
        {
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            PhoneNumber = phoneNumber.Trim(),
            PhoneNumberVerified = false, // Must verify OTP to set true
            PasswordHash = passwordHash
        };
    }

    public void VerifyPhoneNumber()
    {
        PhoneNumberVerified = true;
        Touch();
    }

    public void UpdatePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        Touch();
    }

    public void ChangeRole(string role)
    {
        if (!UserRole.IsValid(role))
            throw new ArgumentException($"Invalid role: {role}");
        Role = role;
        Touch();
    }

    public void Deactivate() { IsActive = false; Touch(); }
    public void Activate()   { IsActive = true;  Touch(); }

    private void Touch() => UpdatedAt = DateTime.UtcNow;
}

/// <summary>Allowed user roles.</summary>
public static class UserRole
{
    public const string Admin = "Admin";
    public const string User  = "User";
    public const string Editor = "Editor";

    public static bool IsValid(string role) =>
        role is Admin or User or Editor;
}
