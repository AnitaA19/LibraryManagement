using LibraryManagement.Core.Entities;
using LibraryManagement.Core.Enums;
using LibraryManagement.DataAccess.Interfaces;

namespace LibraryManagement.Services.BookServices;

public class BookService
{
    private readonly IBookRepository _bookRepository;
    private readonly IBorrowRecordRepository _borrowRecordRepository;
    private readonly IUserRepository _userRepository;

    public BookService(
        IBookRepository bookRepository,
        IBorrowRecordRepository borrowRecordRepository,
        IUserRepository userRepository)
    {
        _bookRepository = bookRepository;
        _borrowRecordRepository = borrowRecordRepository;
        _userRepository = userRepository;
    }

    private void isAdmin(int userId)
    {
        var user = _userRepository.GetEntity(userId);
        if (user.UserRole != UserRole.Admin)
        {
            throw new UnauthorizedAccessException("Only admins can add books.");
        }
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
        isAdmin(userId);

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

    public BookEntity ReturnBook(int id)
    {
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
        isAdmin(userId);
        if (newBook.Isbn == null || newBook.Title == null || newBook.Author == null || newBook.Quantity < 0)
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
        isAdmin(userId);

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
