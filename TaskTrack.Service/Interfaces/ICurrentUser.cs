namespace TaskTrack.Service.Interfaces;

/// <summary>The account making the current request (from the validated JWT).</summary>
public interface ICurrentUser
{
    /// <summary>Null when the request is anonymous.</summary>
    int? AccountId { get; }

    bool IsAdmin { get; }
}
