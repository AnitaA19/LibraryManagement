using System;
using System.IO;

namespace LibraryManagement.Core.Logging;

public static class ErrorLogger
{
    private static readonly string DefaultLogDirectory = Path.Combine(AppContext.BaseDirectory, "Logs");
    private static readonly string LogDirectory = ResolveCoreLogsDirectory() ?? DefaultLogDirectory;
    private static readonly string LogPath = Path.Combine(LogDirectory, "errors.log");

    private static string? ResolveCoreLogsDirectory()
    {
        try
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            for (int i = 0; i < 8 && dir != null; i++)
            {
                var candidate = Path.Combine(dir.FullName, "LibraryManagement.Core", "Logs");
                if (Directory.Exists(candidate))
                {
                    return candidate;
                }
                dir = dir.Parent;
            }
        }
        catch
        {
            // ignore
        }

        return null;
    }

    public static void LogError(string message, Exception? ex = null)
    {
        try
        {
            if (!Directory.Exists(LogDirectory))
            {
                Directory.CreateDirectory(LogDirectory);
            }

            var entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR: {message}";
            if (ex != null)
            {
                entry += Environment.NewLine + ex.ToString();
            }

            File.AppendAllText(LogPath, entry + Environment.NewLine + Environment.NewLine);
        }
        catch
        {
            // Best-effort logger: swallow any exceptions to avoid breaking application flow
        }
    }
}
