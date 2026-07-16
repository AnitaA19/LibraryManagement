namespace LibraryManagement.Core.Exceptions;

public class InvalidVerificationCodeException : LibraryException
{
    public InvalidVerificationCodeException(string message = "Invalid verification code.")
        : base(message)
    {
    }
}