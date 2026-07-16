using LibraryManagement.Core.Entities;
using LibraryManagement.Core.Enums;
using LibraryManagement.Core.Exceptions;
using LibraryManagement.DataAccess.Interfaces;

namespace LibraryManagement.Services.Auth;

public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly EmailService _emailService;

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
            throw new DuplicateEntityException("A user with this username or email already exists.");
        }

        string verificationCode = new Random().Next(100000, 999999).ToString();

        var user = new UserEntity
        {
            Username = username,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            VerificationCode = verificationCode,
            UserRole = UserRole.Client,
        };

        _userRepository.AddEntity(user);
        SendVerificationCode(user.Email, verificationCode);
        Console.WriteLine("Registered");
    }

    public UserEntity SeedInitialAdmin(string username, string email, string password)
    {
        var adminExists = _userRepository
            .GetEntities()
            .Any(x => x.UserRole == UserRole.Admin);

        if (adminExists)
        {
            throw new DuplicateEntityException("An admin user already exists.");
        }

        var admin = new UserEntity
        {
            Username = username,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            UserRole = UserRole.Admin,
            IsVerified = true,
        };

        _userRepository.AddEntity(admin);
        return admin;
    }

    public UserEntity PromoteToAdmin(int actingUserId, int targetUserId)
    {
        var actingUser = _userRepository.GetEntity(actingUserId);
        if (actingUser.UserRole != UserRole.Admin)
        {
            throw new InsufficientPermissionException("Only admins can promote other users to admin.");
        }

        var targetUser = _userRepository.GetEntity(targetUserId);
        targetUser.UserRole = UserRole.Admin;
        _userRepository.UpdateEntity(targetUser);

        return targetUser;
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
            throw new NotFoundException("User was not found.");
        }

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            throw new InvalidCredentialsException();
        }

        if (!user.IsVerified)
        {
            throw new UnverifiedAccountException();
        }

        return user;
    }

    public void SendVerificationCode(string email, string verificationCode)
    {
        _emailService.SeedEmail(email, "Verification code", verificationCode);
    }

    public bool VerifyStudent(string username, string verificationCode)
    {
        var user = _userRepository
            .GetEntities()
            .FirstOrDefault(x => x.Username == username);

        if (user == null)
        {
            throw new NotFoundException("User was not found.");
        }

        if (user.VerificationCode != verificationCode)
        {
            throw new InvalidVerificationCodeException();
        }

        user.IsVerified = true;
        _userRepository.UpdateEntity(user);
        return true;
    }
}