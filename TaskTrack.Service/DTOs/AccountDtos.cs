using System.ComponentModel.DataAnnotations;

namespace TaskTrack.Service.DTOs;

/// <summary>Account as exposed by the API — never includes the password hash.</summary>
public class AccountDto
{
    public int AccountId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public short Role { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
}

/// <summary>Account as seen by an admin, with the number of tasks it has created (blocks deletion when &gt; 0).</summary>
public class AccountDetailDto : AccountDto
{
    public int CreatedTaskCount { get; set; }
}

/// <summary>Admin update: change the name and/or the role. Omitted fields are left unchanged.</summary>
public class UpdateAccountRequest : IValidatableObject
{
    [StringLength(100, ErrorMessage = "Full name must not exceed 100 characters.")]
    public string? FullName { get; set; }

    [Range(0, 1, ErrorMessage = "Role must be 0 (Staff) or 1 (Admin).")]
    public short? Role { get; set; }

    // Member names are the JSON (camelCase) names so they match the other field-level error keys.
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (FullName is null && Role is null)
            yield return new ValidationResult("Provide fullName and/or role to update.", ["fullName"]);
        else if (FullName is not null && string.IsNullOrWhiteSpace(FullName))
            yield return new ValidationResult("Full name cannot be empty.", ["fullName"]);
    }
}
