using System;

namespace LibraryManagement.Services.Auth
{
    public class EmailService
    {
        private static readonly string LogPath =
            Path.Combine(AppContext.BaseDirectory, "Database", "Notifications.log");

        public void SeedEmail(string to, string subject, string body)
        {
            var entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] To: {to} | Subject: {subject} | Body: {body}";

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[SIMULATED EMAIL] {entry}");
            Console.ResetColor();

            try
            {
                var directory = Path.GetDirectoryName(LogPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.AppendAllText(LogPath, entry + Environment.NewLine);
            }
            catch (IOException ex)
            {
                Console.WriteLine($"[WARNING] Could not write to notification log: {ex.Message}");
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"[WARNING] Could not write to notification log: {ex.Message}");
            }
        }
    }
}