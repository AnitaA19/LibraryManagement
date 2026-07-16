using LibraryManagement.Core.Enums;
using LibraryManagement.Core.Exceptions;

namespace LibraryManagement.Core.Entities;

public class UserEntity : BaseEntity
{
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public string Email { get; set; }
    public UserRole UserRole { get; set; } = UserRole.Client;
    private decimal _fines { get; set; }

    public bool IsVerified { get; set; }
    public string VerificationCode { get; set; }

    public decimal Fines
    {
        get { return _fines; }
        set
        {
            if (value < 0)
            {
                throw new ValidationException("Fines cannot be negative.");
            }

            _fines = value;
        }
    }
}