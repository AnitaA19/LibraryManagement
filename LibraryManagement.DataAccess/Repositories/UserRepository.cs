using LibraryManagement.Core.Entities;
using LibraryManagement.Core.Interfaces;

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

    public int RemoveStaleUnverifiedUsers(TimeSpan maxAge)
    {
        var entities = ReadEntitiesFromFile();
        var cutoff = DateTime.UtcNow - maxAge;
        var toRemove = entities.Where(u => !u.IsVerified && (u.CreatedAt == default || u.CreatedAt < cutoff)).ToList();
        foreach (var r in toRemove)
        {
            entities.Remove(r);
        }

        if (toRemove.Count > 0)
        {
            WriteEntitiesToFile(entities);
        }

        return toRemove.Count;
    }

    public int RemoveUnverifiedUsers(bool excludeAdmins)
    {
        var entities = ReadEntitiesFromFile();
        var toRemove = entities.Where(u => !u.IsVerified && (!excludeAdmins || u.UserRole != LibraryManagement.Core.Enums.UserRole.Admin)).ToList();
        foreach (var r in toRemove)
        {
            entities.Remove(r);
        }

        if (toRemove.Count > 0)
        {
            WriteEntitiesToFile(entities);
        }

        return toRemove.Count;
    }
}