using LibraryManagement.Core.Entities;
using LibraryManagement.Core.Interfaces;
using LibraryManagement.Services.Auth;
using LibraryManagement.Services.Interfaces;

namespace LibraryManagement.App.UI
{
    internal class ClientMenu : BaseMenu
    {
        private readonly IBookService _bookService;
        private readonly IBookRepository _bookRepository;
        private readonly AuthService _authService;
        private readonly IUserService _userService;

        public ClientMenu(
            IBookService bookService,
            IBookRepository bookRepository,
            SessionContext session,
            AuthService authService,
            IUserService userService) : base(session)
        {
            _bookService = bookService;
            _bookRepository = bookRepository;
            _authService = authService;
            _userService = userService;
        }

        protected override int MaxOption => 9;

        protected override void DisplayMenu(UserEntity currentUser)
        {
            Console.WriteLine($"=== Client Menu ({currentUser.Username}) ===");
            Console.WriteLine("1) View books");
            Console.WriteLine("2) Search books");
            Console.WriteLine("3) Borrow a book (request)");
            Console.WriteLine("4) Return a book");
            Console.WriteLine("5) View my borrow records");
            Console.WriteLine("6) View my fines");
            Console.WriteLine("7) Pay fines");
            Console.WriteLine("8) Update email");
            Console.WriteLine("9) Change password");
            Console.WriteLine("0) Logout");
        }

        protected override void HandleChoice(int choice, UserEntity currentUser)
        {
            switch (choice)
            {
                case 1:
                    ConsoleIO.RunSafely(ViewBooks);
                    break;
                case 2:
                    ConsoleIO.RunSafely(SearchBooks);
                    break;
                case 3:
                    ConsoleIO.RunSafely(() => BorrowBook(currentUser));
                    break;
                case 4:
                    ConsoleIO.RunSafely(() => ReturnBook(currentUser));
                    break;
                case 5:
                    ConsoleIO.RunSafely(() => ViewMyBorrowRecords(currentUser));
                    break;
                case 6:
                    ConsoleIO.RunSafely(() => ViewMyFines(currentUser));
                    break;
                case 7:
                    ConsoleIO.RunSafely(() => PayFines(currentUser));
                    break;
                case 8:
                    ConsoleIO.RunSafely(() => UpdateEmail(currentUser));
                    break;
            }
        }

        private void ViewBooks()
        {
            var pageNumber = ConsoleIO.ReadInt("Page number: ");
            var books = _bookService.ViewBooks(pageNumber);
            PrintBooks(books);
        }

        private void SearchBooks()
        {
            var term = ConsoleIO.ReadNonEmptyString("Search term: ");
            var books = _bookService.SearchBooks(term);
            PrintBooks(books);
        }

        private void BorrowBook(UserEntity currentUser)
        {
            var bookId = ConsoleIO.ReadInt("Book Id: ");
            var returnDate = ConsoleIO.ReadDate("Return date");
            _bookService.BorrowBookRequest(currentUser.Id, bookId, returnDate);
            ConsoleIO.WriteSuccess("Borrow request submitted, awaiting admin approval.");
        }

        private void ReturnBook(UserEntity currentUser)
        {
            var recordId = ConsoleIO.ReadInt("Borrow record Id: ");
            var book = _bookService.ReturnBook(currentUser.Id, recordId);
            ConsoleIO.WriteSuccess($"\"{book.Title}\" returned.");
        }

        private void ViewMyBorrowRecords(UserEntity currentUser)
        {
            var records = _bookService.GetBorrowRecordsForUser(currentUser.Id);
            foreach (var record in records)
            {
                var title = _bookRepository.FindBookByIsbn(record.Isbn)?.Title ?? "Unknown book";
                Console.WriteLine($"[{record.Id}] \"{title}\" - {record.BorrowStatus} - Due: {record.ReturnDate:yyyy-MM-dd}");
            }
        }

        private void ViewMyFines(UserEntity currentUser)
        {
            Console.WriteLine($"Outstanding fines: {currentUser.Fines:0.00}");
        }

        private void PayFines(UserEntity currentUser)
        {
            var amount = ConsoleIO.ReadDecimal("Amount to pay: ");
            var remaining = _userService.PayFines(currentUser.Id, amount);
            ConsoleIO.WriteSuccess($"Payment accepted. Remaining fines: {remaining:0.00}");
        }

        private void UpdateEmail(UserEntity currentUser)
        {
            var newEmail = ConsoleIO.ReadNonEmptyString("New email: ");
            _authService.UpdateEmail(currentUser.Id, newEmail);
            ConsoleIO.WriteSuccess("Email updated. A verification code has been sent to your new email.");
        }

        private void ChangePassword(UserEntity currentUser)
        {
            var current = ConsoleIO.ReadNonEmptyString("Current password: ");
            var nw = ConsoleIO.ReadNonEmptyString("New password: ");
            _authService.ChangePassword(currentUser.Id, current, nw);
            ConsoleIO.WriteSuccess("Password changed successfully.");
        }

        private void PrintBooks(IEnumerable<BookEntity> books)
        {
            foreach (var book in books)
            {
                Console.WriteLine($"[{book.Id}] {book.Title} by {book.Author} (ISBN: {book.Isbn}) - Qty: {book.Quantity}");
            }
        }
    }
}