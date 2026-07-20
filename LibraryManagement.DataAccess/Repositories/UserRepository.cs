using LibraryManagement.Core.Entities;
using LibraryManagement.DataAccess.Interfaces;

namespace LibraryManagement.DataAccess.Repositories;

public class UserRepository : BaseRepository<UserEntity>, IUserRepository
{
    private static readonly string DefaultPath = DatabasePathResolver.Resolve("Users.json");

    public UserRepository(string path) : base(path)
    {
    }

    public UserRepository() : base(DefaultPath)
    {
    }
}