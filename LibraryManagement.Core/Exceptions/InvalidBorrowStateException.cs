namespace LibraryManagement.Core.Exceptions;

public class InvalidBorrowStateException : LibraryException
{
    public InvalidBorrowStateException(string message) : base(message)
    {
    }
}