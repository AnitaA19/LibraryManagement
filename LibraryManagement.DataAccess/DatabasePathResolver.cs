namespace LibraryManagement.DataAccess;

internal static class DatabasePathResolver
{
    public static string Resolve(string fileName)
    {
        var repoRoot = FindRepoRoot(AppContext.BaseDirectory);
        var databaseDirectory = repoRoot != null
            ? Path.Combine(repoRoot, "LibraryManagement.DataAccess", "Database")
            : Path.Combine(AppContext.BaseDirectory, "Database");

        return Path.Combine(databaseDirectory, fileName);
    }

    private static string? FindRepoRoot(string startDirectory)
    {
        var directory = new DirectoryInfo(startDirectory);

        while (directory != null)
        {
            if (directory.EnumerateFiles("*.slnx").Any())
            {
                return directory.FullName;
            }
            directory = directory.Parent;
        }

        return null;
    }
}
