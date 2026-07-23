using LibraryManagement.Core.Entities;

namespace LibraryManagement.Core.Interfaces;

public interface IUserRepository : IBaseRepository<UserEntity>
{
    UserEntity GetUserById(int id);
}
