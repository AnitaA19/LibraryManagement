using LibraryManagement.Core.Entities;

namespace LibraryManagement.DataAccess.Interfaces;

public interface IUserRepository : IBaseRepository<UserEntity>
{
    int RemoveStaleUnverifiedUsers(TimeSpan maxAge);
    int RemoveUnverifiedUsers(bool excludeAdmins);
}
