namespace LibraryManagement.Core.Exceptions;

public class DuplicateEntityException : LibraryException
{
    public DuplicateEntityException(string message) : base(message)
    {
    }
}