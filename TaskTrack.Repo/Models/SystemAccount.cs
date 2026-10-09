using System;
using System.Collections.Generic;

namespace TaskTrack.Repo.Models;

public partial class SystemAccount
{
    public int AccountId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public short Role { get; set; }

    public DateTime CreatedDate { get; set; }

    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    public virtual ICollection<Task> TaskCreatedBies { get; set; } = new List<Task>();

    public virtual ICollection<Task> TaskUpdatedBies { get; set; } = new List<Task>();
}
