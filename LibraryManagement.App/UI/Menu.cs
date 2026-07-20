using LibraryManagement.Core.Enums;
using LibraryManagement.DataAccess.Interfaces;
using LibraryManagement.Services.Auth;
using LibraryManagement.Services.BookServices;
using LibraryManagement.Services.Notifications;

namespace LibraryManagement.App.UI
{
    internal class Menu
    {
        private readonly AuthService _authService;
        private readonly BookService _bookService;
        private readonly NotificationService _notificationService;
        private readonly IBookRepository _bookRepository;
        private readonly SessionContext _session;

        public Menu(
            AuthService authService,
            BookService bookService,
            NotificationService notificationService,
            IBookRepository bookRepository,
            SessionContext session)
        {
            _authService = authService;
            _bookService = bookService;
            _notificationService = notificationService;
            _bookRepository = bookRepository;
            _session = session;
        }

        public void Run()
        {
            var isFirstScreen = true;

            while (true)
            {
                if (!_session.IsLoggedIn)
                {
                    if (!isFirstScreen)
                    {
                        ConsoleIO.WaitForKey();
                    }
                    ConsoleIO.Clear();
                    isFirstScreen = false;
                    Console.WriteLine("=== Library Management ===");
                    Console.WriteLine("1) Login");
                    Console.WriteLine("2) Register");
                    Console.WriteLine("3) Exit");
                    var choice = ConsoleIO.ReadMenuChoice("Choose an option: ", 1, 3);

                    switch (choice)
                    {
                        case 1:
                            new LoginPage(_authService, _session).Run();
                            break;
                        case 2:
                            new RegistrationPage(_authService).Run();
                            break;
                        case 3:
                            return;
                    }
                }
                else if (_session.CurrentUser!.UserRole == UserRole.Admin)
                {
                    new AdminMenu(_bookService, _authService, _notificationService, _bookRepository, _session).Run();
                }
                else
                {
                    new ClientMenu(_bookService, _bookRepository, _session, _authService).Run();
                }
            }
        }
    }
}
