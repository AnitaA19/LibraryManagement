using System.Net;
using System.Net.Mail;
using System.Text;

using LibraryManagement.Services.Interfaces;

namespace LibraryManagement.Services.Auth
{
    public class EmailService : IEmailService
    {
        private readonly global::LibraryManagement.Core.Configuration.EmailSettings _settings;
        private static readonly string LogPath =
            Path.Combine(AppContext.BaseDirectory, "Database", "Notifications.log");

        public EmailService(global::LibraryManagement.Core.Configuration.EmailSettings settings)
        {
            _settings = settings;
        }

        public bool SendEmail(string to, string subject, string body)
        {
            var htmlBody = BuildHtmlEmail(subject, body);

            if (!string.IsNullOrEmpty(_settings?.Host) && !string.IsNullOrEmpty(_settings.Username) && !string.IsNullOrEmpty(_settings.Password))
            {
                try
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
                        Body = htmlBody,
                        IsBodyHtml = true,
                        BodyEncoding = Encoding.UTF8
                    };

                    mailMessage.To.Add(to);

                    smtpClient.Send(mailMessage);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Email sent to {to}");
                    Console.ResetColor();
                    return true;
                }
                catch (SmtpException ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Failed to send email to {to}: {ex.Message}");
                    Console.ResetColor();
                    return false;
                }
            }

            var entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] To: {to} | Subject: {subject}\n{body}";
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("[SIMULATED EMAIL]\n" + entry);
            Console.ResetColor();

            try
            {
                var directory = Path.GetDirectoryName(LogPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.AppendAllText(LogPath, entry + Environment.NewLine);
                return true;
            }
            catch (IOException ex)
            {
                Console.WriteLine($"[WARNING] Could not write to notification log: {ex.Message}");
                return false;
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.WriteLine($"[WARNING] Could not write to notification log: {ex.Message}");
                return false;
            }
        }

        public static string BuildHtmlEmail(string title, string message, string ctaText = null, string ctaUrl = null)
        {
            var cta = string.Empty;
            if (!string.IsNullOrEmpty(ctaText) && !string.IsNullOrEmpty(ctaUrl))
            {
                cta = $"<p style=\"text-align:center;margin:18px 0;\"><a href=\"{System.Net.WebUtility.HtmlEncode(ctaUrl)}\" style=\"background:#7c3aed;color:#fff;padding:12px 20px;border-radius:8px;text-decoration:none;\">{System.Net.WebUtility.HtmlEncode(ctaText)}</a></p>";
            }

            // Brown-themed, serious library style
            return $@"<!doctype html>
<html>
<head>
  <meta charset=""utf-8""> 
  <meta name=""viewport"" content=""width=device-width,initial-scale=1""> 
  <title>{System.Net.WebUtility.HtmlEncode(title)}</title>
</head>
<body style=""font-family:Georgia,serif;background:#efe6dd;padding:24px;color:#2b1f16;""> 
  <table width=""100%"" cellpadding=""0"" cellspacing=""0"" role=""presentation""> 
    <tr><td align=""center""> 
      <table width=""640"" style=""max-width:96%;background:#fff;border-radius:8px;overflow:hidden;border:1px solid #e0d4c8;""> 
        <tr><td style=""background:linear-gradient(90deg,#5b3b2e,#8b5e3c);padding:20px 24px;color:#fff;""> 
          <h1 style=""margin:0;font-size:22px;font-family:Georgia,serif;letter-spacing:0.6px;"">{System.Net.WebUtility.HtmlEncode(title)}</h1>
        </td></tr>
        <tr><td style=""padding:22px;font-family:Arial,Helvetica,sans-serif;color:#3b2d25;line-height:1.5;""> 
          <p style=""margin:0 0 14px;font-size:15px;"">{System.Net.WebUtility.HtmlEncode(message)}</p>
          {cta}
          <hr style=""border:none;border-top:1px solid #e6d9cf;margin:18px 0;"" />
          <p style=""color:#6b5347;font-size:13px;margin:0;"">If you did not request this, or believe this message was sent in error, please contact library support.</p>
        </td></tr>
        <tr><td style=""padding:12px 18px;background:#fbf7f4;color:#6b5347;font-size:12px;text-align:center;"">LibraryManagement • Bringing curated knowledge to your community</td></tr>
      </table>
    </td></tr>
  </table>
</body>
</html>";
        }
    }
}
