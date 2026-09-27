namespace TaskTrack.Repo.Repositories;

internal static class SearchPattern
{
    /// <summary>Builds an ILIKE pattern for a partial match, escaping LIKE wildcards in the input.</summary>
    public static string Contains(string value) =>
        "%" + value.Trim().Replace(@"\", @"\\").Replace("%", @"\%").Replace("_", @"\_") + "%";
}
