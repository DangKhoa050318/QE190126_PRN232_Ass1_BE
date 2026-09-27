namespace TaskTrack.Service.Common;

/// <summary>Display names for the numeric status/priority codes stored in the database.</summary>
public static class Labels
{
    public static readonly string[] ProjectStatuses = ["Not Started", "In Progress", "Completed", "On Hold"];
    public static readonly string[] TaskStatuses = ["To Do", "In Progress", "Done", "Cancelled"];
    public static readonly string[] TaskPriorities = ["Low", "Medium", "High", "Critical"];

    public static string ProjectStatus(short value) => Get(ProjectStatuses, value);
    public static string TaskStatus(short value) => Get(TaskStatuses, value);
    public static string TaskPriority(short value) => Get(TaskPriorities, value);

    private static string Get(string[] names, short value) =>
        value >= 0 && value < names.Length ? names[value] : "Unknown";
}
