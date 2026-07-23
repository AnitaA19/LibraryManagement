using LibraryManagement.Core.Entities;
using LibraryManagement.Core.Enums;
using LibraryManagement.Core.Exceptions;
using LibraryManagement.Core.Interfaces;
using LibraryManagement.Services.Interfaces;

namespace LibraryManagement.Services.BookServices;

public class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;
    private readonly IBorrowRecordRepository _borrowRecordRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUserService _userService;

    public BookService(
        IBookRepository bookRepository,
        IBorrowRecordRepository borrowRecordRepository,
        IUserRepository userRepository,
        IUserService userService)
    {
        _bookRepository = bookRepository;
        _borrowRecordRepository = borrowRecordRepository;
        _userRepository = userRepository;
        _userService = userService;
    }

    private bool IsAdmin(int userId)
    {
        var user = _userRepository.GetEntity(userId);
        return user.UserRole == UserRole.Admin;
    }

    private decimal ValidateFines(int userId)
    {
        return _userService.ValidateFines(userId);
    }

    public IEnumerable<BookEntity> ViewBooks(int pageNumber = 1, int pageSize = 10)
    {
        if (pageNumber < 1 || pageSize < 1)
        {
            throw new ValidationException("Page number and page size must be positive.");
        }

        return _bookRepository
            .GetEntities()
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
    }

    public IEnumerable<BookEntity> ViewBooksForUser(int userId, int pageNumber = 1, int pageSize = 10)
    {
        var result = ViewBooks(pageNumber, pageSize);
        LibraryManagement.Services.Logging.EventLogger.Log($"Books viewed: UserId={userId}, Page={pageNumber}, PageSize={pageSize}, Returned={result.Count()} items");
        return result;
    }

    public IEnumerable<BookEntity> SearchBooks(string searchTerm, int pageNumber = 1, int pageSize = 10)
    {
        if (pageNumber < 1 || pageSize < 1)
        {
            throw new ValidationException("Page number and page size must be positive.");
        }

        return _bookRepository
            .GetEntities()
            .Where(b =>
                b.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                b.Author.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
    }
    public IEnumerable<BookEntity> SearchBooksForUser(int userId, string searchTerm, int pageNumber = 1, int pageSize = 10)
    {
        var result = SearchBooks(searchTerm, pageNumber, pageSize);
        LibraryManagement.Services.Logging.EventLogger.Log($"Books searched: UserId={userId}, Term='{searchTerm}', Page={pageNumber}, PageSize={pageSize}, Returned={result.Count()} items");
        return result;
    }

    public BorrowRecordEntity BorrowBookRequest(int userId, int id, DateTime returnDate)
    {
        ValidateFines(userId);

        var book = _bookRepository.GetEntity(id);
        if (book.Quantity <= 0)
        {
            throw new BookUnavailableException($"\"{book.Title}\" is not available for borrowing.");
        }

        if (returnDate <= DateTime.Now)
        {
            throw new ValidationException("Return date must be in the future.");
        }

        var borrowRecord = new BorrowRecordEntity
        {
            UserId = userId,
            Isbn = book.Isbn,
            ReturnDate = returnDate,
            BorrowStatus = BorrowStatus.Pending
        };

        _borrowRecordRepository.AddEntity(borrowRecord);

        LibraryManagement.Services.Logging.EventLogger.Log($"Borrow request created: UserId={userId}, Isbn={borrowRecord.Isbn}, ReturnDate={borrowRecord.ReturnDate:yyyy-MM-dd}, RequestId={borrowRecord.Id}");

        return borrowRecord;
    }

    public BorrowRecordEntity BorrowBookApprove(int id, int userId)
    {
        if (!IsAdmin(userId))
        {
            throw new InsufficientPermissionException("Only admins can approve borrow requests.");
        }

        var borrowRecord = _borrowRecordRepository.GetEntity(id);
        if (borrowRecord.BorrowStatus != BorrowStatus.Pending)
        {
            throw new InvalidBorrowStateException("Only pending borrow requests can be approved.");
        }
        borrowRecord.BorrowStatus = BorrowStatus.Approved;
        var book = _bookRepository.GetBookByIsbn(borrowRecord.Isbn);
        book.DecreaseQuantity(1);
        _borrowRecordRepository.UpdateEntity(borrowRecord);
        _bookRepository.UpdateEntity(book);
        LibraryManagement.Services.Logging.EventLogger.Log($"Borrow approved: RequestId={borrowRecord.Id}, UserId={borrowRecord.UserId}, Isbn={borrowRecord.Isbn}");
        return borrowRecord;
    }

    public BorrowRecordEntity BorrowBookReject(int id, int userId)
    {
        if (!IsAdmin(userId))
        {
            throw new InsufficientPermissionException("Only admins can reject borrow requests.");
        }

        var borrowRecord = _borrowRecordRepository.GetEntity(id);
        if (borrowRecord.BorrowStatus != BorrowStatus.Pending)
        {
            throw new InvalidBorrowStateException("Only pending borrow requests can be rejected.");
        }
        borrowRecord.BorrowStatus = BorrowStatus.Rejected;
        _borrowRecordRepository.UpdateEntity(borrowRecord);
        LibraryManagement.Services.Logging.EventLogger.Log($"Borrow rejected: RequestId={borrowRecord.Id}, UserId={borrowRecord.UserId}, Isbn={borrowRecord.Isbn}");
        return borrowRecord;
    }

    public IEnumerable<BorrowRecordEntity> GetBorrowRecordsForUser(int userId)
    {
        return _borrowRecordRepository.GetEntities().Where(r => r.UserId == userId);
    }

    public IEnumerable<BorrowRecordEntity> GetAllBorrowRecords(int actingUserId)
    {
        if (!IsAdmin(actingUserId))
        {
            throw new InsufficientPermissionException("Only admins can view all borrow records.");
        }
        return _borrowRecordRepository.GetEntities();
    }

    public BookEntity ReturnBook(int userId, int id)
    {
        ValidateFines(userId);

        var borrowRecord = _borrowRecordRepository.GetEntity(id);
        if (borrowRecord.BorrowStatus != BorrowStatus.Approved)
        {
            throw new InvalidBorrowStateException("Only approved (currently borrowed) books can be returned.");
        }
        borrowRecord.BorrowStatus = BorrowStatus.Returned;
        var book = _bookRepository.GetBookByIsbn(borrowRecord.Isbn);
        book.IncreaseQuantity(1);
        _borrowRecordRepository.UpdateEntity(borrowRecord);
        _bookRepository.UpdateEntity(book);
        LibraryManagement.Services.Logging.EventLogger.Log($"Book returned: RequestId={borrowRecord.Id}, UserId={borrowRecord.UserId}, Isbn={borrowRecord.Isbn}");
        return book;
    }

    public BookEntity AddBook(BookEntity newBook, int userId)
    {
        if (!IsAdmin(userId))
        {
            throw new InsufficientPermissionException("Only admins can add books.");
        }

        if (
            string.IsNullOrWhiteSpace(newBook.Isbn) ||
            string.IsNullOrWhiteSpace(newBook.Title) ||
            string.IsNullOrWhiteSpace(newBook.Author)
            )
        {
            throw new ValidationException("Invalid book details.");
        }

        var book = _bookRepository.FindBookByIsbn(newBook.Isbn);
        if (book == null)
        {
            _bookRepository.AddEntity(newBook);
            return newBook;
        }

        book.IncreaseQuantity(newBook.Quantity);
        _bookRepository.UpdateEntity(book);

        return newBook;
    }

    public void DeleteBook(int id, int userId, int quantity)
    {
        if (!IsAdmin(userId))
        {
            throw new InsufficientPermissionException("Only admins can delete books.");
        }

        var book = _bookRepository.GetEntity(id);

        if (quantity <= 0 || quantity > book.Quantity)
        {
            throw new ValidationException("Invalid quantity to delete.");
        }

        if (quantity == book.Quantity)
        {
            _bookRepository.DeleteEntity(id);
        }
        else
        {
            book.DecreaseQuantity(quantity);
            _bookRepository.UpdateEntity(book);
        }
    }
}