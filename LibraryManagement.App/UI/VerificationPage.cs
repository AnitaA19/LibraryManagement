using LibraryManagement.Core.Exceptions;
using LibraryManagement.Services.Auth;

namespace LibraryManagement.App.UI
{
    internal class VerificationPage
    {
        private readonly AuthService _authService;

        public VerificationPage(AuthService authService)
        {
            _authService = authService;
        }

        public void Run()
        {
            Console.WriteLine();
            Console.WriteLine("--- Verification ---");

            var email = ConsoleIO.ReadNonEmptyString("Email: ");

            var code = ConsoleIO.ReadNonEmptyString("Verification code: ");

            try
            {
                _authService.VerifyStudent(email, code);
                ConsoleIO.WriteSuccess("Account verified. You can now log in.");
            }
            catch (LibraryException ex)
            {
                ConsoleIO.WriteError(ex.Message);
            }
        }
    }
}
