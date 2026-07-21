using LibraryManagement.Core.Exceptions;
using LibraryManagement.Services.Auth;

namespace LibraryManagement.App.UI
{
    internal class LoginPage
    {
        private readonly AuthService _authService;
        private readonly SessionContext _session;

        public LoginPage(AuthService authService, SessionContext session)
        {
            _authService = authService;
            _session = session;
        }

        public void Run()
        {
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine("--- Login ---");
                var email = ConsoleIO.ReadNonEmptyString("Email: ");
                var password = ConsoleIO.ReadNonEmptyString("Password: ");

                try
                {
                    var user = _authService.Login(email, password);
                    _session.CurrentUser = user;
                    ConsoleIO.WriteSuccess($"Welcome, {user.Username}!");
                    return;
                }
                catch (LibraryException ex)
                {
                    ConsoleIO.WriteError(ex.Message);
                    var retry = ConsoleIO.ReadNonEmptyString("Try again? (y/n): ");
                    if (!retry.Equals("y", StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }
                }
            }
        }
    }
}

