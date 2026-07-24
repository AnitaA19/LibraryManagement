using System;
using System.IO;

namespace LibraryManagement.Core.Logging;

public static class EventLogger
{
    private static readonly string DefaultLogPath = Path.Combine(AppContext.BaseDirectory, "Database", "Events.log");
    private static readonly string LogPath = ResolveCoreLogPath("event.log") ?? DefaultLogPath;

    private static string? ResolveCoreLogPath(string fileName)
    {
        try
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            for (int i = 0; i < 8 && dir != null; i++)
            {
                var candidate = Path.Combine(dir.FullName, "LibraryManagement.Core", "Logs");
                if (Directory.Exists(candidate))
                {
                    return Path.Combine(candidate, fileName);
                }
                dir = dir.Parent;
            }
        }
        catch
        {
            // ignore and fallback
        }

        return null;
    }

    public static void Log(string message)
    {
        try
        {
            var directory = Path.GetDirectoryName(LogPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}";
            File.AppendAllText(LogPath, entry);
        }
        catch (Exception ex)
        {
            // If event logging fails, write to error logger as a fallback
            try
            {
                ErrorLogger.LogError("Failed to write event log.", ex);
            }
            catch
            {
                // swallow
            }
        }
    }
}
