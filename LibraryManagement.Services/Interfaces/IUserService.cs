using LibraryManagement.Core.Entities;

namespace LibraryManagement.Services.Interfaces;

public interface IUserService
{
    int RemoveStaleUnverifiedUsers(TimeSpan maxAge);
    int RemoveUnverifiedUsers(bool excludeAdmins);

    void ApplyOverdueFines();
    decimal ValidateFines(int userId);
    decimal PayFines(int userId, decimal amount);
}
