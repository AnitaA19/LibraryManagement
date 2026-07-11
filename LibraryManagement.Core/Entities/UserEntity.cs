using LibraryManagement.Core.Enums;

namespace LibraryManagement.Core.Entities;

public class UserEntity : BaseEntity
{
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public string Email { get; set; }
    public UserRole UserRole { get; set; } = UserRole.Client;
    private double _fines { get; set; }

    public double Fines
    {
        get { return _fines; }
        set
        {
            if (value < 0)
            {
                throw new ArgumentException("Fines cannot be negative.");
            }

            _fines = value;
        }
    }
}
