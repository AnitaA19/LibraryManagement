using LibraryManagement.Core.Enums;

namespace LibraryManagement.Core.Entities;

public class BorrowRecordEntity : BaseEntity
{
    // Foreign key to UserEntity
    public int UserId { get; set; }
    // Foreign key to BookEntity
    public string Isbn { get; set; }
    public DateTime ReturnDate { get; set; }
    public BorrowStatus BorrowStatus { get; set; }
    public int FinedDays { get; set; }
}