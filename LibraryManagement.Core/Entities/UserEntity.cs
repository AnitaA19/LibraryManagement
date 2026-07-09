using LibraryManagement.Core.Enums;

namespace LibraryManagement.Core.Entities;

public class UserEntity : BaseEntity 
{
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public string Email { get; set; }
    public UserRole UserRole { get; set; } = UserRole.Client;
    public double Fines { get; set; }
}
