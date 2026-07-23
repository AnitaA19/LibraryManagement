using LibraryManagement.Core.Entities;
using LibraryManagement.Core.Enums;

namespace LibraryManagement.Services.Interfaces;

public interface IBookService
{
    IEnumerable<BookEntity> ViewBooks(int pageNumber = 1, int pageSize = 10);
    IEnumerable<BookEntity> ViewBooksForUser(int userId, int pageNumber = 1, int pageSize = 10);
    IEnumerable<BookEntity> SearchBooks(string searchTerm, int pageNumber = 1, int pageSize = 10);
    IEnumerable<BookEntity> SearchBooksForUser(int userId, string searchTerm, int pageNumber = 1, int pageSize = 10);

    BorrowRecordEntity BorrowBookRequest(int userId, int id, DateTime returnDate);
    BorrowRecordEntity BorrowBookApprove(int id, int userId);
    BorrowRecordEntity BorrowBookReject(int id, int userId);

    IEnumerable<BorrowRecordEntity> GetBorrowRecordsForUser(int userId);
    IEnumerable<BorrowRecordEntity> GetAllBorrowRecords(int actingUserId);

    BookEntity ReturnBook(int userId, int id);

    BookEntity AddBook(BookEntity newBook, int userId);
    void DeleteBook(int id, int userId, int quantity);
}
