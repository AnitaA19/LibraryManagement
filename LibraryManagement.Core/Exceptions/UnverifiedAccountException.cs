namespace LibraryManagement.Core.Exceptions;

public class UnverifiedAccountException : LibraryException
{
    public UnverifiedAccountException(string message = "Account is not verified yet. Please check your email for the verification code.")
        : base(message)
    {
    }
}