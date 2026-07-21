using LibraryManagement.Core.Exceptions;
using LibraryManagement.Services.Auth;
using System.Net.Mail;

namespace LibraryManagement.App.UI
{
    internal class RegistrationPage
    {
        private readonly AuthService _authService;

        public RegistrationPage(AuthService authService)
        {
            _authService = authService;
        }

        public void Run()
        {
            Console.WriteLine();
            Console.WriteLine("--- Register ---");

            var email = TryRegister();
            if (email != null)
            {
                Verify(email);
            }
        }

        private string? TryRegister()
        {
            while (true)
            {
                var username = ConsoleIO.ReadNonEmptyString("Username: ");
                var email = ConsoleIO.ReadNonEmptyString("Email: ");
                var password = ConsoleIO.ReadNonEmptyString("Password: ");
                if (!IsValidEmail(email))
                {
                    ConsoleIO.WriteError("Invalid email format. Please provide a valid email like user@example.com.");
                    var retry = ConsoleIO.ReadNonEmptyString("Try again? (y/n): ");
                    if (!retry.Equals("y", StringComparison.OrdinalIgnoreCase))
                    {
                        return null;
                    }
                    continue;
                }

                try
                {
                    _authService.RegisterUser(username, email, password);
                    Console.WriteLine("A verification code has been sent (see simulated email above).");
                    return email;
                }
                catch (DuplicateEntityException ex)
                {
                    ConsoleIO.WriteError(ex.Message);
                    var goToVerify = ConsoleIO.ReadNonEmptyString(
                        "If this account exists but isn't verified yet, verify it now? (y/n): ");
                    if (goToVerify.Equals("y", StringComparison.OrdinalIgnoreCase))
                    {
                        return email;
                    }
                    return null;
                }
                catch (LibraryException ex)
                {
                    ConsoleIO.WriteError(ex.Message);
                    var retry = ConsoleIO.ReadNonEmptyString("Try again? (y/n): ");
                    if (!retry.Equals("y", StringComparison.OrdinalIgnoreCase))
                    {
                        return null;
                    }
                }
            }
        }

        private void Verify(string email)
        {
            while (true)
            {
                var code = ConsoleIO.ReadNonEmptyString("Enter verification code: ");

                try
                {
                    _authService.VerifyStudent(email, code);
                    ConsoleIO.WriteSuccess("Account verified. You can now log in.");
                    return;
                }
                catch (LibraryException ex)
                {
                    ConsoleIO.WriteError(ex.Message);
                    var retry = ConsoleIO.ReadNonEmptyString("Try again? (y/n): ");
                    if (!retry.Equals("y", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("You can verify later by registering again.");
                        return;
                    }
                }
            }
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new MailAddress(email);
                var host = addr.Host ?? string.Empty;
                if (!host.Contains('.'))
                {
                    return false;
                }
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}
