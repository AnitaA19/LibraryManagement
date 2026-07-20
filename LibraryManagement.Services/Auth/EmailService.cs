using System.Net;
using System.Net.Mail;

namespace LibraryManagement.Services.Auth
{
    public class EmailService
    {
        private readonly global::LibraryManagement.Core.Entities.EmailSettings _settings;
        private static readonly string LogPath =
            Path.Combine(AppContext.BaseDirectory, "Database", "Notifications.log");

        public EmailService(global::LibraryManagement.Core.Entities.EmailSettings settings)
        {
            _settings = settings;
        }

        public void SeedEmail(string to, string subject, string body)
        {
            // Try to send real email if configuration provided; otherwise fallback to simulated logging
            if (!string.IsNullOrEmpty(_settings?.Host) && !string.IsNullOrEmpty(_settings.Username) && !string.IsNullOrEmpty(_settings.Password))
            {
                using var smtpClient = new SmtpClient(_settings.Host, _settings.Port)
                {
                    EnableSsl = _settings.EnableSsl,
                    Credentials = new NetworkCredential(_settings.Username, _settings.Password)
                };

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(_settings.FromEmail, _settings.FromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false
                };

                mailMessage.To.Add(to);

                try
                {
                    smtpClient.Send(mailMessage);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Email sent to {to}");
                    Console.ResetColor();
                    return;
                }
                catch (SmtpException ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Failed to send email to {to}: {ex.Message}");
                    Console.ResetColor();
                    // Fall through to log the simulated email as a fallback
                }
            }

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
