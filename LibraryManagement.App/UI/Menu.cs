using LibraryManagement.Core.Exceptions;
using LibraryManagement.Services.Auth;

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

            var username = TryRegister();
            if (username != null)
            {
                Verify(username);
            }
        }

        private string? TryRegister()
        {
            while (true)
            {
                var username = ConsoleIO.ReadNonEmptyString("Username: ");
                var email = ConsoleIO.ReadNonEmptyString("Email: ");
                var password = ConsoleIO.ReadNonEmptyString("Password: ");

                try
                {
                    _authService.RegisterUser(username, email, password);
                    Console.WriteLine("A verification code has been sent (see simulated email above).");
                    return username;
                }
                catch (DuplicateEntityException ex)
                {
                    ConsoleIO.WriteError(ex.Message);
                    var goToVerify = ConsoleIO.ReadNonEmptyString(
                        "If this account exists but isn't verified yet, verify it now? (y/n): ");
                    if (goToVerify.Equals("y", StringComparison.OrdinalIgnoreCase))
                    {
                        return username;
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

        private void Verify(string username)
        {
            while (true)
            {
                var code = ConsoleIO.ReadNonEmptyString("Enter verification code: ");

                try
                {
                    _authService.VerifyStudent(username, code);
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
    }
}
