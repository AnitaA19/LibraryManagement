using LibraryManagement.Core.Entities;
using LibraryManagement.DataAccess.Interfaces;
namespace LibraryManagement.DataAccess.Repositories;

public class BookRepository : BaseRepository<BookEntity>, IBookRepository
{
    public BookRepository(string path) : base(path)
    {
        path.Concat("Books.json");
    }

    public BookRepository() : base(@"C:\Users\balas\source\repos\LibraryManagement\LibraryManagement.DataAccess\Database\Books.json")
    {
    }

    public BookEntity GetBookByIsbn(string isbn)
    {
        var books = ReadEntitiesFromFile();
        return books.FirstOrDefault(b => b.Isbn == isbn) ??
            throw new Exception($"{typeof(BookEntity).Name} not found.");
    }
}
