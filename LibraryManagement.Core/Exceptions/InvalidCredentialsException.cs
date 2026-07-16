namespace LibraryManagement.Core.Exceptions;

public class InvalidCredentialsException : LibraryException
{
    public InvalidCredentialsException(string message = "Invalid username/email or password.")
        : base(message)
    {
    }
}