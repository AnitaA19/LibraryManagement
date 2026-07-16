namespace LibraryManagement.Core.Exceptions;

public class DataAccessException : LibraryException
{
    public DataAccessException(string message) : base(message)
    {
    }

    public DataAccessException(string message, Exception innerException) : base(message, innerException)
    {
    }
}