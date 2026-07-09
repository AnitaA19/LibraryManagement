using LibraryManagement.Core.Entities;
using LibraryManagement.DataAccess.Repositories;

namespace LibraryManagement.Services.Auth;

public class AuthService
{
    private readonly BaseRepository<UserEntity> _userRepository;

    public AuthService(BaseRepository<UserEntity> userRepository)
    {
        _userRepository = userRepository;
    }

    public void RegisterUser(string username, string email, string password)
    {
        var existingUser = _userRepository
            .GetEntities()
            .FirstOrDefault(x =>
                x.Username == username ||
                x.Email == email);

        if (existingUser != null)
        {
            throw new Exception("User already exists.");
        }

        var user = new UserEntity
        {
            Username = username,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
        };

        _userRepository.AddEntity(user);
    }

    public UserEntity Login(string usernameOrEmail, string password)
    {
        var user = _userRepository
            .GetEntities()
            .FirstOrDefault(x =>
                x.Username == usernameOrEmail ||
                x.Email == usernameOrEmail);

        if (user == null)
        {
            throw new Exception("User not found.");
        }

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            throw new Exception("Invalid password.");
        }

        return user;
    }
}