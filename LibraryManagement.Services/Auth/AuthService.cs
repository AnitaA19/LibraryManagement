using LibraryManagement.Core.Entities;
using LibraryManagement.Core.Enums;
using LibraryManagement.Core.Exceptions;
using LibraryManagement.Core.Interfaces;
using System.Net.Mail;
using LibraryManagement.Services.Logging;
using LibraryManagement.Services.Interfaces;

namespace LibraryManagement.Services.Auth;

public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly LibraryManagement.Services.Interfaces.IEmailService _emailService;
    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, UserEntity> _pending =
        new(System.StringComparer.OrdinalIgnoreCase);

    public AuthService(IUserRepository userRepository, IEmailService emailService)
    {
        _userRepository = userRepository;
        _emailService = emailService;
    }

    public void RegisterUser(string username, string email, string password)
    {
        ValidateEmailFormat(email);
        var existingUser = _userRepository
            .GetEntities()
            .FirstOrDefault(x =>
                x.Email == email);

        if (existingUser != null)
        {
            throw new DuplicateEntityException("A user with this email already exists.");
        }

        string verificationCode = new Random().Next(100000, 999999).ToString();

        if (string.IsNullOrWhiteSpace(verificationCode))
        {
            throw new ValidationException("Failed to generate verification code. Registration aborted.");
        }

        var user = new UserEntity
        {
            Username = username,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            VerificationCode = verificationCode,
            UserRole = UserRole.Client,
        };

        user.CreatedAt = DateTime.UtcNow;
        user.VerificationSentAt = DateTime.UtcNow;

        var emailSent = _emailService.SendEmail(email, "Verification code", verificationCode);
        if (!emailSent)
        {
            throw new ValidationException("Failed to send verification email. Registration aborted.");
        }

        // Store pending registration in-memory until verification completes.
        _pending[email] = user;
        // SendVerificationCode(user.Email, verificationCode);
    }

    public UserEntity SeedInitialAdmin(string username, string email, string password)
    {
        ValidateEmailFormat(email);

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
        EventLogger.Log($"Admin seeded: Email={email}, Username={username}");
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
        // Clear fines when promoting to admin
        targetUser.ClearFines();
        _userRepository.UpdateEntity(targetUser);
        EventLogger.Log($"User promoted to admin: ActingUserId={actingUserId}, TargetUserId={targetUserId}");

        return targetUser;
    }

    public UserEntity Login(string usernameOrEmail, string password)
    {
        // Login is only allowed via email
        var user = _userRepository
            .GetEntities()
            .FirstOrDefault(x => x.Email == usernameOrEmail);

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

        EventLogger.Log($"User logged in: UserId={user.Id}, Email={user.Email}");
        return user;
    }

    public void SendVerificationCode(string email, string verificationCode)
    {
        _emailService.SendEmail(email, "Verification code", verificationCode);
        EventLogger.Log($"Verification code sent: Email={email}");
    }

    public bool VerifyStudent(string username, string verificationCode)
    {

        var email = username;


        var user = _userRepository.GetEntities().FirstOrDefault(x => x.Email == email);

        if (user != null)
        {
            // existing persisted user
            if (user.VerificationCode != verificationCode)
            {
                throw new InvalidVerificationCodeException();
            }

            if (user.VerificationSentAt.HasValue && DateTime.UtcNow - user.VerificationSentAt.Value > TimeSpan.FromMinutes(10))
            {
                throw new InvalidVerificationCodeException("Verification code expired.");
            }

            user.IsVerified = true;
            _userRepository.UpdateEntity(user);
            EventLogger.Log($"User verified (existing): UserId={user.Id}, Email={user.Email}");
            return true;
        }


        if (!_pending.TryGetValue(email, out var pendingUser))
        {
            throw new NotFoundException("User was not found.");
        }

        if (pendingUser.VerificationCode != verificationCode)
        {
            throw new InvalidVerificationCodeException();
        }

        if (pendingUser.VerificationSentAt.HasValue && DateTime.UtcNow - pendingUser.VerificationSentAt.Value > TimeSpan.FromMinutes(10))
        {
            throw new InvalidVerificationCodeException("Verification code expired.");
        }


        pendingUser.IsVerified = true;
        _userRepository.AddEntity(pendingUser);
        _pending.TryRemove(email, out _);
        EventLogger.Log($"User verified (new): Email={email}, AssignedId={pendingUser.Id}");
        return true;
    }

    public void UpdateEmail(int userId, string newEmail)
    {
        ValidateEmailFormat(newEmail);
        var user = _userRepository.GetEntity(userId);

        var duplicate = _userRepository.GetEntities().Any(u => u.Email == newEmail && u.Id != userId);
        if (duplicate)
        {
            throw new DuplicateEntityException("A user with this email already exists.");
        }

        user.Email = newEmail;
        user.IsVerified = false;
        var verificationCode = new Random().Next(100000, 999999).ToString();
        user.VerificationCode = verificationCode;

        _userRepository.UpdateEntity(user);
        SendVerificationCode(newEmail, verificationCode);
        EventLogger.Log($"User email updated: UserId={userId}, NewEmail={newEmail}");
    }

    public void ChangePassword(int userId, string currentPassword, string newPassword)
    {
        var user = _userRepository.GetEntity(userId);

        if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
        {
            throw new InvalidCredentialsException("Current password is incorrect.");
        }

        if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
        {
            throw new ValidationException("New password must be at least 6 characters long.");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        _userRepository.UpdateEntity(user);
        EventLogger.Log($"User changed password: UserId={userId}");
    }

    private void ValidateEmailFormat(string email)
    {
        try
        {
            var addr = new MailAddress(email);
            if (addr.Address != email)
            {
                throw new ValidationException("Invalid email format.");
            }
        }
        catch
        {
            throw new ValidationException("Invalid email format.");
        }
    }
}