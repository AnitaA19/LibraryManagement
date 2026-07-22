using LibraryManagement.Core.Entities;
using LibraryManagement.Core.Interfaces;
using LibraryManagement.Services.Auth;
using LibraryManagement.Services.BookServices;
using LibraryManagement.Services.Notifications;

namespace LibraryManagement.App.UI
{
    internal class AdminMenu : BaseMenu
    {
        private readonly BookService _bookService;
        private readonly AuthService _authService;
        private readonly NotificationService _notificationService;
        private readonly IBookRepository _bookRepository;

        public AdminMenu(
            BookService bookService,
            AuthService authService,
            NotificationService notificationService,
            IBookRepository bookRepository,
            SessionContext session) : base(session)
        {
            _bookService = bookService;
            _authService = authService;
            _notificationService = notificationService;
            _bookRepository = bookRepository;
        }

        protected override int MaxOption => 10;

        protected override void DisplayMenu(UserEntity currentUser)
        {
            Console.WriteLine($"=== Admin Menu ({currentUser.Username}) ===");
            Console.WriteLine("1) View books");
            Console.WriteLine("2) Search books");
            Console.WriteLine("3) Add book");
            Console.WriteLine("4) Delete book / reduce quantity");
            Console.WriteLine("5) Approve pending borrow request");
            Console.WriteLine("6) Reject pending borrow request");
            Console.WriteLine("7) View all borrow records");
            Console.WriteLine("8) View overdue records");
            Console.WriteLine("9) Promote user to admin");
            Console.WriteLine("10) Run due-date notification sweep");
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
                    ConsoleIO.RunSafely(() => AddBook(currentUser));
                    break;
                case 4:
                    ConsoleIO.RunSafely(() => DeleteBook(currentUser));
                    break;
                case 5:
                    ConsoleIO.RunSafely(() => ApproveBorrowRequest(currentUser));
                    break;
                case 6:
                    ConsoleIO.RunSafely(() => RejectBorrowRequest(currentUser));
                    break;
                case 7:
                    ConsoleIO.RunSafely(() => ViewAllBorrowRecords(currentUser));
                    break;
                case 8:
                    ConsoleIO.RunSafely(ViewOverdueRecords);
                    break;
                case 9:
                    ConsoleIO.RunSafely(() => PromoteToAdmin(currentUser));
                    break;
                case 10:
                    ConsoleIO.RunSafely(RunNotificationSweep);
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

        private void AddBook(UserEntity currentUser)
        {
            var isbn = ConsoleIO.ReadNonEmptyString("ISBN: ");
            var title = ConsoleIO.ReadNonEmptyString("Title: ");
            var author = ConsoleIO.ReadNonEmptyString("Author: ");
            var quantity = ConsoleIO.ReadInt("Quantity: ");

            var book = new BookEntity(isbn, title, author, quantity);

            _bookService.AddBook(book, currentUser.Id);
            ConsoleIO.WriteSuccess($"Book \"{title}\" added.");
        }

        private void DeleteBook(UserEntity currentUser)
        {
            var bookId = ConsoleIO.ReadInt("Book Id: ");
            var quantity = ConsoleIO.ReadInt("Quantity to remove: ");
            _bookService.DeleteBook(bookId, currentUser.Id, quantity);
            ConsoleIO.WriteSuccess("Book updated/removed.");
        }

        private void ApproveBorrowRequest(UserEntity currentUser)
        {
            var recordId = ConsoleIO.ReadInt("Borrow record Id: ");
            _bookService.BorrowBookApprove(recordId, currentUser.Id);
            ConsoleIO.WriteSuccess("Borrow request approved.");
        }

        private void RejectBorrowRequest(UserEntity currentUser)
        {
            var recordId = ConsoleIO.ReadInt("Borrow record Id: ");
            _bookService.BorrowBookReject(recordId, currentUser.Id);
            ConsoleIO.WriteSuccess("Borrow request rejected.");
        }

        private void ViewAllBorrowRecords(UserEntity currentUser)
        {
            var records = _bookService.GetAllBorrowRecords(currentUser.Id);
            PrintBorrowRecords(records);
        }

        private void ViewOverdueRecords()
        {
            var records = _notificationService.GetOverdueRecords();
            PrintBorrowRecords(records);
        }

        private void PromoteToAdmin(UserEntity currentUser)
        {
            var targetUserId = ConsoleIO.ReadInt("User Id to promote: ");
            var target = _authService.PromoteToAdmin(currentUser.Id, targetUserId);
            ConsoleIO.WriteSuccess($"{target.Username} is now an admin.");
        }

        private void RunNotificationSweep()
        {
            var (dueSoonCount, overdueCount) = _notificationService.SendDueDateNotifications();
            ConsoleIO.WriteSuccess($"Notifications sent: {dueSoonCount} due tomorrow, {overdueCount} overdue.");
        }

        private void PrintBooks(IEnumerable<BookEntity> books)
        {
            foreach (var book in books)
            {
                Console.WriteLine($"[{book.Id}] {book.Title} by {book.Author} (ISBN: {book.Isbn}) - Qty: {book.Quantity}");
            }
        }

        private void PrintBorrowRecords(IEnumerable<BorrowRecordEntity> records)
        {
            foreach (var record in records)
            {
                var title = _bookRepository.FindBookByIsbn(record.Isbn)?.Title ?? "Unknown book";
                string userEmail;
                try
                {
                    var user = _authService.GetUserById(record.UserId);
                    userEmail = user.Email;
                }
                catch
                {
                    userEmail = $"User {record.UserId}";
                }

                Console.WriteLine($"[{record.Id}] {userEmail} - \"{title}\" - {record.BorrowStatus} - Due: {record.ReturnDate:yyyy-MM-dd}");
            }
        }
    }
}