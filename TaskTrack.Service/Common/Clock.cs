namespace TaskTrack.Service.Common;

public static class Clock
{
    /// <summary>
    /// Current local time for "timestamp without time zone" columns. Kind is Unspecified so the value
    /// serializes the same way as values read back from the database (no offset suffix).
    /// </summary>
    public static DateTime Now => DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Unspecified);
}
