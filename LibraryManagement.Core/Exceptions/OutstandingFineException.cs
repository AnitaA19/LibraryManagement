namespace LibraryManagement.Core.Exceptions;

public class OutstandingFineException : LibraryException
{
    public decimal Amount { get; }

    public OutstandingFineException(decimal amount)
        : base($"User has unpaid fines: {amount:0.00}")
    {
        Amount = amount;
    }
}