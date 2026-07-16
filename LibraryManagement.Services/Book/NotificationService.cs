using LibraryManagement.Core.Entities;
using LibraryManagement.Core.Enums;
using LibraryManagement.DataAccess.Interfaces;
using LibraryManagement.Services.Auth;
using LibraryManagement.Services.BookServices;

namespace LibraryManagement.Services.Notifications;

public class NotificationService
{
    private readonly IBorrowRecordRepository _borrowRecordRepository;
    private readonly IUserRepository _userRepository;
    private readonly IBookRepository _bookRepository;
    private readonly EmailService _emailService;
    private readonly BookService _bookService;

    public NotificationService(
        IBorrowRecordRepository borrowRecordRepository,
        IUserRepository userRepository,
        IBookRepository bookRepository,
        EmailService emailService,
        BookService bookService)
    {
        _borrowRecordRepository = borrowRecordRepository;
        _userRepository = userRepository;
        _bookRepository = bookRepository;
        _emailService = emailService;
        _bookService = bookService;
    }

    public IEnumerable<BorrowRecordEntity> GetRecordsDueTomorrow()
    {
        var tomorrow = DateTime.Now.Date.AddDays(1);

        return _borrowRecordRepository.GetEntities()
            .Where(r => r.BorrowStatus == BorrowStatus.Approved && r.ReturnDate.Date == tomorrow);
    }

    public IEnumerable<BorrowRecordEntity> GetOverdueRecords()
    {
        var today = DateTime.Now.Date;

        return _borrowRecordRepository.GetEntities()
            .Where(r => r.BorrowStatus == BorrowStatus.Approved && r.ReturnDate.Date < today);
    }

    public (int dueSoonCount, int overdueCount) SendDueDateNotifications()
    {
        _bookService.ApplyOverdueFines();

        int dueSoonCount = 0;
        foreach (var record in GetRecordsDueTomorrow())
        {
            NotifyDueSoon(record);
            dueSoonCount++;
        }

        int overdueCount = 0;
        foreach (var record in GetOverdueRecords())
        {
            NotifyOverdue(record);
            overdueCount++;
        }

        return (dueSoonCount, overdueCount);
    }

    private void NotifyDueSoon(BorrowRecordEntity record)
    {
        var user = _userRepository.GetEntity(record.UserId);
        var bookTitle = _bookRepository.FindBookByIsbn(record.Isbn)?.Title ?? "Unknown book";

        var subject = "Reminder: your book is due tomorrow";
        var body = $"Hi {user.Username}, \"{bookTitle}\" is due back tomorrow ({record.ReturnDate:yyyy-MM-dd}). " +
                   "Please return it on time to avoid a fine.";

        _emailService.SeedEmail(user.Email, subject, body);
    }

    private void NotifyOverdue(BorrowRecordEntity record)
    {
        var user = _userRepository.GetEntity(record.UserId);
        var bookTitle = _bookRepository.FindBookByIsbn(record.Isbn)?.Title ?? "Unknown book";
        var daysLate = (DateTime.Now.Date - record.ReturnDate.Date).Days;

        var subject = "Your book is overdue";
        var body = $"Hi {user.Username}, \"{bookTitle}\" was due on {record.ReturnDate:yyyy-MM-dd} " +
                   $"and is now {daysLate} day(s) late. Current outstanding fines: {user.Fines:0.00}. " +
                   "Please return the book and settle any fines as soon as possible.";

        _emailService.SeedEmail(user.Email, subject, body);
    }
}