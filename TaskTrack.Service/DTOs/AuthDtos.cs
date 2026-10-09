using System.ComponentModel.DataAnnotations;

namespace TaskTrack.Service.DTOs;

public class RegisterRequest
{
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(100, ErrorMessage = "Full name must not exceed 100 characters.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Email is not a valid email address.")]
    [StringLength(255, ErrorMessage = "Email must not exceed 255 characters.")]
    public string Email { get; set; } = string.Empty;

    // BCrypt only uses the first 72 bytes of a password, hence the upper limit.
    [Required(ErrorMessage = "Password is required.")]
    [StringLength(72, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 72 characters.")]
    public string Password { get; set; } = string.Empty;
}

public class LoginRequest
{
    [Required(ErrorMessage = "Email is required.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = string.Empty;
}

public class UpdateProfileRequest
{
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(100, ErrorMessage = "Full name must not exceed 100 characters.")]
    public string FullName { get; set; } = string.Empty;
}

public class ChangePasswordRequest : IValidatableObject
{
    [Required(ErrorMessage = "Current password is required.")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "New password is required.")]
    [StringLength(72, MinimumLength = 8, ErrorMessage = "New password must be between 8 and 72 characters.")]
    public string NewPassword { get; set; } = string.Empty;

    // Member name is the JSON (camelCase) name so it matches the other field-level error keys.
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (NewPassword == CurrentPassword)
            yield return new ValidationResult("New password must be different from the current password.", ["newPassword"]);
    }
}

public class RefreshTokenRequest
{
    [Required(ErrorMessage = "Refresh token is required.")]
    public string RefreshToken { get; set; } = string.Empty;
}

/// <summary>Returned by login and refresh: the signed JWT, a refresh token and the account they belong to.</summary>
public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public DateTime ExpiresAt { get; set; }

    /// <summary>Single-use token for POST /api/auth/refresh; each refresh returns a new one.</summary>
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpiresAt { get; set; }

    public AccountDto Account { get; set; } = new();
}
