namespace LibraryManagement.Core.Exceptions;

public class InsufficientPermissionException : LibraryException
{
    public InsufficientPermissionException(string message) : base(message)
    {
    }
}