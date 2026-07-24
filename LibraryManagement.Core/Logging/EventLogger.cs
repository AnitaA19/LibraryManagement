using System;
using System.IO;

namespace LibraryManagement.Core.Logging;

public static class EventLogger
{
    private static readonly string LogPath = Path.Combine(AppContext.BaseDirectory, "Database", "Events.log");

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
