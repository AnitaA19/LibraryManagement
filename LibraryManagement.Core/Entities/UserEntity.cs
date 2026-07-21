using LibraryManagement.Core.Enums;
using LibraryManagement.Core.Exceptions;
using System.Text.Json.Serialization;

namespace LibraryManagement.Core.Entities;

public class UserEntity : BaseEntity
{
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public string Email { get; set; }
    public UserRole UserRole { get; set; } = UserRole.Client;

    private decimal _fines;

    public bool IsVerified { get; set; }
    public string VerificationCode { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? VerificationSentAt { get; set; }

    [JsonInclude]
    public decimal Fines
    {
        get => _fines;
        private set
        {
            if (value < 0)
            {
                throw new ValidationException("Fines cannot be negative.");
            }

            _fines = value;
        }
    }

    public void AddFine(decimal amount)
    {
        if (UserRole == UserRole.Admin)
        {
            throw new ValidationException("Admins cannot have fines.");
        }
        if (amount <= 0)
        {
            throw new ValidationException("Fine amount must be positive.");
        }

        Fines += amount;
    }

    public void PayFine(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ValidationException("Payment amount must be positive.");
        }

        if (amount > Fines)
        {
            throw new ValidationException("Payment amount exceeds total fines.");
        }

        Fines -= amount;
    }

    public void ClearFines()
    {
        Fines = 0m;
    }
}