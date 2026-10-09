namespace TaskTrack.Service.Common;

/// <summary>SystemAccount.Role values. The role name is what goes into the JWT "role" claim.</summary>
public static class AccountRoles
{
    public const short Staff = 0;
    public const short Admin = 1;

    public const string StaffName = "Staff";
    public const string AdminName = "Admin";

    public static string Name(short role) => role == Admin ? AdminName : StaffName;
}
