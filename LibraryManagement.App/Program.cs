using LibraryManagement.App.UI;
using LibraryManagement.Core.Enums;
using LibraryManagement.DataAccess.Repositories;
using LibraryManagement.Services.Auth;
using LibraryManagement.Services.BookServices;
using LibraryManagement.Services.Notifications;

var bookRepository = new BookRepository();
var userRepository = new UserRepository();
var borrowRecordRepository = new BorrowRecordRepository();

var emailService = new EmailService();
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
    var username = ConsoleIO.ReadNonEmptyString("Admin username: ");
    var email = ConsoleIO.ReadNonEmptyString("Admin email: ");
    var password = ConsoleIO.ReadNonEmptyString("Admin password: ");

    authService.SeedInitialAdmin(username, email, password);
    ConsoleIO.WriteSuccess("Admin account created. You can log in now.");
}
