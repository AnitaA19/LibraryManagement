namespace LibraryManagement.Core.Exceptions;

public class ValidationException : LibraryException
{
    public ValidationException(string message) : base(message)
    {
    }
}