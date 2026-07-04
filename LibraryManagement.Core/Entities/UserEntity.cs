using LibraryManagement.Core.Enums;

namespace LibraryManagement.Core.Entities;

public class UserEntity : BaseEntity 
{
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public UserRole UserRole { get; set; }
    public double Fines { get; set; }
}
