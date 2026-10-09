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
