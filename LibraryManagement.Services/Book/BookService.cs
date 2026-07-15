using LibraryManagement.Core.Entities;
using LibraryManagement.Core.Enums;
using LibraryManagement.DataAccess.Interfaces;

namespace LibraryManagement.Services.BookServices;


public class BookService
{
    private readonly IBookRepository _bookRepository;
    private readonly IBorrowRecordRepository _borrowRecordRepository;
    private readonly IUserRepository _userRepository;
    private readonly int FinePerDay = 5;

    public BookService(
        IBookRepository bookRepository,
        IBorrowRecordRepository borrowRecordRepository,
        IUserRepository userRepository)
    {
        _bookRepository = bookRepository;
        _borrowRecordRepository = borrowRecordRepository;
        _userRepository = userRepository;
    }

    private bool IsAdmin(int userId)
    {
        var user = _userRepository.GetEntity(userId)
            ?? throw new Exception("User was not found");
        return user.UserRole == UserRole.Admin;
    }

    private decimal ValidateFines(int userId)
    {
        var user = _userRepository.GetEntity(userId);

        if (user.Fines > 0)
        {
            throw new Exception($"User has unpaid fines: {user.Fines}");
        }
        return user.Fines;
    }

    public decimal ComputeFines(int userId)
    {
        var user = _userRepository.GetEntity(userId);
        var borrowRecords = _borrowRecordRepository.GetEntities()
            .Where(br => br.UserId == userId && br.BorrowStatus == BorrowStatus.Approved);
        decimal totalFines = 0;
        foreach (var record in borrowRecords)
        {
            if (record.ReturnDate < DateTime.Now)
            {
                var overdueDays = (DateTime.Now - record.ReturnDate).Days;
                totalFines += overdueDays * FinePerDay;
            }
        }
        user.Fines = totalFines;
        _userRepository.UpdateEntity(user);
        return totalFines;
    }

    public decimal PayFines(int userId, decimal amount)
    {
        var user = _userRepository.GetEntity(userId);
        if (amount <= 0)
        {
            throw new Exception("Payment amount must be positive.");
        }
        if (amount > user.Fines)
        {
            throw new Exception("Payment amount exceeds total fines.");
        }
        user.Fines -= amount;
        _userRepository.UpdateEntity(user);
        return user.Fines;
    }

    public IEnumerable<BookEntity> ViewBooks(int pageNumber = 10, int pageSize = 1)
    {
        return _bookRepository
            .GetEntities()
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
    }

    public IEnumerable<BookEntity> SearchBooks(string searchTerm, int pageNumber = 10, int pageSize = 1)
    {
        return _bookRepository
            .GetEntities()
            .Where(b => b.Title.Contains(searchTerm) || b.Author.Contains(searchTerm))
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);
    }

    public BorrowRecordEntity BorrowBookRequest(int userId, int id, DateTime returnDate)
    {
        decimal fine = ValidateFines(userId);

        var book = _bookRepository.GetEntity(id);
        if (book.Quantity <= 0)
        {
            throw new Exception("Book not available for borrowing.");
        }

        if (returnDate <= DateTime.Now)
        {
            throw new Exception("Return date must be in the future.");
        }

        var borrowRecord = new BorrowRecordEntity
        {
            UserId = userId,
            Isbn = book.Isbn,
            ReturnDate = returnDate,
            BorrowStatus = BorrowStatus.Pending
        };

        _borrowRecordRepository.AddEntity(borrowRecord);

        return borrowRecord;
    }

    public BorrowRecordEntity BorrowBookApprove(int id, int userId)
    {
        if (!IsAdmin(userId))
        {
            throw new Exception("Only admins can approve borrow requests.");
        }

        var borrowRecord = _borrowRecordRepository.GetEntity(id);
        if (borrowRecord == null || borrowRecord.BorrowStatus != BorrowStatus.Pending)
        {
            throw new Exception("Invalid borrow request.");
        }
        borrowRecord.BorrowStatus = BorrowStatus.Approved;
        var book = _bookRepository.GetBookByIsbn(borrowRecord.Isbn);
        book.Quantity -= 1;
        _borrowRecordRepository.UpdateEntity(borrowRecord);
        _bookRepository.UpdateEntity(book);
        return borrowRecord;
    }

    public BookEntity ReturnBook(int userId, int id)
    {
        decimal fine = ValidateFines(userId);

        var borrowRecord = _borrowRecordRepository.GetEntity(id);
        if (borrowRecord == null || borrowRecord.BorrowStatus != BorrowStatus.Approved)
        {
            throw new Exception("Invalid return request.");
        }
        borrowRecord.BorrowStatus = BorrowStatus.Returned;
        var book = _bookRepository.GetBookByIsbn(borrowRecord.Isbn);
        book.Quantity += 1;
        _borrowRecordRepository.UpdateEntity(borrowRecord);
        _bookRepository.UpdateEntity(book);
        return book;
    }

    public BookEntity AddBook(BookEntity newBook, int userId)
    {
        if (!IsAdmin(userId))
        {
            throw new Exception("Only admins can add books.");
        }

        if (
            string.IsNullOrWhiteSpace(newBook.Isbn) ||
            string.IsNullOrWhiteSpace(newBook.Title) ||
            string.IsNullOrWhiteSpace(newBook.Author)
            )
        {
            throw new Exception("Invalid book details.");
        }

        var book = _bookRepository.GetBookByIsbn(newBook.Isbn);

        if (book == null)
        {
            _bookRepository.AddEntity(newBook);
            return newBook;
        }

        book.Quantity += newBook.Quantity;
        _bookRepository.UpdateEntity(book);

        return newBook;
    }

    public void DeleteBook(int id, int userId, int quantity)
    {
        if (IsAdmin(userId))
        {
            throw new Exception("Only admins can delete books.");
        }

        var book = _bookRepository.GetEntity(id);

        if (quantity <= 0 || quantity > book.Quantity)
        {
            throw new Exception("Invalid quantity to delete.");
        }

        if (quantity == book.Quantity)
        {
            _bookRepository.DeleteEntity(id);
        }
        else
        {
            book.Quantity -= quantity;
            _bookRepository.UpdateEntity(book);
        }
    }
}