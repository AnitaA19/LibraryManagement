using LibraryManagement.Core.Entities;
using LibraryManagement.DataAccess.Interfaces;

namespace LibraryManagement.DataAccess.Repositories;

public class BorrowRecordRepository : BaseRepository<BorrowRecordEntity>, IBorrowRecordRepository
{
    private static readonly string DefaultPath =
        Path.Combine(AppContext.BaseDirectory, "Database", "BorrowRecords.json");

    public BorrowRecordRepository(string path) : base(path)
    {
    }

    public BorrowRecordRepository() : base(DefaultPath)
    {
    }
}