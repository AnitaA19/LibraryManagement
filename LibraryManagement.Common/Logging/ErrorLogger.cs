using System;
using System.IO;

namespace LibraryManagement.Common.Logging
{
    public static class ErrorLogger
    {
        private static readonly string LogDirectory = Path.Combine(AppContext.BaseDirectory, "Logs");
        private static readonly string LogPath = Path.Combine(LogDirectory, "errors.log");

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
}
