using LibraryManagement.Core.Entities;
using LibraryManagement.DataAccess.Interfaces;

namespace LibraryManagement.Services.Auth;

public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly EmailService _emailService = new EmailService();

    public AuthService(IUserRepository userRepository, EmailService emailService)
    {
        _userRepository = userRepository;
        _emailService = emailService;
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

        string verificationCode = new Random().Next(100000, 999999).ToString();

        var user = new UserEntity
        {
            Username = username,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            VerificationCode = verificationCode,
        };

        _userRepository.AddEntity(user);
        SendVerificationCode(username, verificationCode);
        Console.WriteLine("Registered");
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

    public void SendVerificationCode(string username, string verificationCode)
    {
        _emailService.SeedEmail(username, "Verification code", verificationCode);
    }

    public  bool VerifyStudent(string username, string verificationCode)
    {
        var user = _userRepository
            .GetEntities()
            .FirstOrDefault(x => x.Username == username);
        if (user == null)
        {
            throw new Exception("User not found.");
        }
        if (user.VerificationCode != verificationCode)
        {
            throw new Exception("Invalid verification code.");
        }
        user.IsVerified = true;
        _userRepository.UpdateEntity(user);
        return true;
    }
}