using LibraryManagement.App.UI;
using LibraryManagement.Core.Enums;
using LibraryManagement.DataAccess.Repositories;
using LibraryManagement.Services.Auth;
using LibraryManagement.Services.BookServices;
using LibraryManagement.Services.Auth;
using LibraryManagement.Services.Notifications;
using Microsoft.Extensions.Configuration;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddUserSecrets<Program>()
    .Build();

var emailSettings = new EmailSettings();
configuration.GetSection("Email").Bind(emailSettings);

var bookRepository = new BookRepository();
var userRepository = new UserRepository();
var borrowRecordRepository = new BorrowRecordRepository();

var emailService = new EmailService(emailSettings);
var authService = new AuthService(userRepository, emailService);
var bookService = new BookService(bookRepository, borrowRecordRepository, userRepository);
var notificationService = new NotificationService(borrowRecordRepository, userRepository, bookRepository, emailService, bookService);

EnsureAdminSeeded(authService, userRepository);

var session = new SessionContext();
new Menu(authService, bookService, notificationService, bookRepository, session).Run();

Console.WriteLine("Goodbye!");

static void EnsureAdminSeeded(AuthService authService, LibraryManagement.DataAccess.Interfaces.IUserRepository userRepository)
{
    var adminExists = userRepository.GetEntities().Any(u => u.UserRole == UserRole.Admin);
    if (adminExists)
    {
        return;
    }

    Console.WriteLine("No admin account exists yet. Let's create one.");

    while (true)
    {
        var username = ConsoleIO.ReadNonEmptyString("Admin username: ");
        var email = ConsoleIO.ReadNonEmptyString("Admin email: ");
        var password = ConsoleIO.ReadNonEmptyString("Admin password: ");

        try
        {
            authService.SeedInitialAdmin(username, email, password);
            ConsoleIO.WriteSuccess("Admin account created. You can log in now.");
            break;
        }
        catch (LibraryManagement.Core.Exceptions.ValidationException ex)
        {
            ConsoleIO.WriteError(ex.Message + " Please try again.");
            var retry = ConsoleIO.ReadNonEmptyString("Try again? (y/n): ");
            if (!retry.Equals("y", StringComparison.OrdinalIgnoreCase))
            {
                ConsoleIO.WriteError("No admin was created. Exiting.");
                Environment.Exit(0);
            }
        }
        catch (LibraryManagement.Core.Exceptions.LibraryException ex)
        {
            ConsoleIO.WriteError(ex.Message);
            var retry = ConsoleIO.ReadNonEmptyString("Try again? (y/n): ");
            if (!retry.Equals("y", StringComparison.OrdinalIgnoreCase))
            {
                ConsoleIO.WriteError("No admin was created. Exiting.");
                Environment.Exit(0);
            }
        }
    }
}
