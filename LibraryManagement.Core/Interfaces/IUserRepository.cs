using LibraryManagement.Core.Entities;

namespace LibraryManagement.Core.Interfaces;

public interface IUserRepository : IBaseRepository<UserEntity>
{
    int RemoveStaleUnverifiedUsers(TimeSpan maxAge);
    int RemoveUnverifiedUsers(bool excludeAdmins);
    UserEntity GetUserById(int id);
}
