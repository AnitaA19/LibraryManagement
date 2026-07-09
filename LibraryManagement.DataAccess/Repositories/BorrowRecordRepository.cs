
using LibraryManagement.Core.Entities;
using LibraryManagement.DataAccess.Interfaces;

namespace LibraryManagement.DataAccess.Repositories;

public class BorrowRecordRepository : BaseRepository<BorrowRecordEntity>, IBorrowRecordRepository
{
    public BorrowRecordRepository(string path) : base(path)
    {
        path.Concat("BorrowRecords.json");
    }

    public BorrowRecordRepository() : base(@"C:\Users\balas\source\repos\LibraryManagement\LibraryManagement.DataAccess\Database\BorrowRecords.json")
    {
    }
}
