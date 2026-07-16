namespace LibraryManagement.Core.Exceptions;

public class BookUnavailableException : LibraryException
{
    public BookUnavailableException(string message = "Book is not available for borrowing.")
        : base(message)
    {
    }
}