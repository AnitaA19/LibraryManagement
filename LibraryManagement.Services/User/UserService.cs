using System.Linq;
using LibraryManagement.Core.Entities;
using LibraryManagement.Core.Enums;
using LibraryManagement.Core.Interfaces;
using LibraryManagement.Core.Exceptions;
using LibraryManagement.Services.Interfaces;

namespace LibraryManagement.Services.User;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IBorrowRecordRepository _borrowRecordRepository;
    private const int FinePerDay = 5;

    public UserService(IUserRepository userRepository, IBorrowRecordRepository borrowRecordRepository)
    {
        _userRepository = userRepository;
        _borrowRecordRepository = borrowRecordRepository;
    }

    public int RemoveStaleUnverifiedUsers(TimeSpan maxAge)
    {
        var entities = _userRepository.GetEntities().ToList();
        var cutoff = DateTime.UtcNow - maxAge;
        var toRemove = entities.Where(u => !u.IsVerified && (u.CreatedAt == default || u.CreatedAt < cutoff)).ToList();

        foreach (var r in toRemove)
        {
            entities.Remove(r);
        }

        if (toRemove.Count > 0)
        {
            foreach (var rem in toRemove)
            {
                try
                {
                    _userRepository.DeleteEntity(rem.Id);
                }
                catch
                {
                    // ignore failures on delete
                }
            }
        }

        return toRemove.Count;
    }

    public int RemoveUnverifiedUsers(bool excludeAdmins)
    {
        var entities = _userRepository.GetEntities().ToList();
        var toRemove = entities.Where(u => !u.IsVerified && (!excludeAdmins || u.UserRole != UserRole.Admin)).ToList();

        foreach (var r in toRemove)
        {
            entities.Remove(r);
        }

        if (toRemove.Count > 0)
        {
            foreach (var rem in toRemove)
            {
                try
                {
                    _userRepository.DeleteEntity(rem.Id);
                }
                catch
                {
                    // ignore
                }
            }
        }

        return toRemove.Count;
    }

    public void ApplyOverdueFines()
    {
        var overdueRecords = _borrowRecordRepository.GetEntities()
            .Where(br => br.BorrowStatus == BorrowStatus.Approved && br.ReturnDate < DateTime.Now);

        foreach (var record in overdueRecords)
        {
            ApplyOverdueFineToRecord(record);
        }
    }

    public decimal ValidateFines(int userId)
    {
        ApplyOverdueFinesForUser(userId);

        var user = _userRepository.GetEntity(userId);

        if (user.Fines > 0)
        {
            throw new OutstandingFineException(user.Fines);
        }
        return user.Fines;
    }

    private void ApplyOverdueFinesForUser(int userId)
    {
        var overdueRecords = _borrowRecordRepository.GetEntities()
            .Where(br => br.UserId == userId && br.BorrowStatus == BorrowStatus.Approved && br.ReturnDate < DateTime.Now);

        foreach (var record in overdueRecords)
        {
            ApplyOverdueFineToRecord(record);
        }
    }

    private void ApplyOverdueFineToRecord(BorrowRecordEntity record)
    {
        var overdueDays = (DateTime.Now - record.ReturnDate).Days;
        var newOverdueDays = overdueDays - record.FinedDays;

        if (newOverdueDays <= 0)
        {
            return;
        }

        var user = _userRepository.GetEntity(record.UserId);
        user.AddFine(newOverdueDays * FinePerDay);
        record.FinedDays = overdueDays;

        _userRepository.UpdateEntity(user);
        _borrowRecordRepository.UpdateEntity(record);
    }

    public decimal PayFines(int userId, decimal amount)
    {
        var user = _userRepository.GetEntity(userId);
        user.PayFine(amount);
        _userRepository.UpdateEntity(user);
        LibraryManagement.Services.Logging.EventLogger.Log($"Fines paid: UserId={userId}, Amount={amount:0.00}, Remaining={user.Fines:0.00}");
        return user.Fines;
    }
}
