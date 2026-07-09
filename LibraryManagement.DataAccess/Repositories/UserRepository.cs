using LibraryManagement.Core.Entities;
using LibraryManagement.DataAccess.Interfaces;

namespace LibraryManagement.DataAccess.Repositories;

public class UserRepository : BaseRepository<UserEntity>, IUserRepository
{
    public UserRepository(string path) : base(path)
    {
        path.Concat("Users.json");
    }

    public UserRepository() : base(@"C:\Users\balas\source\repos\LibraryManagement\LibraryManagement.DataAccess\Database\Users.json")
    {
    }
}