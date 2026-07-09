using LibraryManagement.Core.Entities;
using LibraryManagement.DataAccess.Interfaces;
namespace LibraryManagement.DataAccess.Repositories;

public class BookRepository : BaseRepository<BookEntity>, IBookRepository
{
    private static readonly string DefaultPath =
        Path.Combine(AppContext.BaseDirectory, "Database", "Books.json");

    public BookRepository(string path) : base(path)
    {
    }

    public BookRepository() : base(DefaultPath)
    {
    }

    public BookEntity GetBookByIsbn(string isbn)
    {
        var books = ReadEntitiesFromFile();
        return books.FirstOrDefault(b => b.Isbn == isbn) ??
            throw new Exception($"{typeof(BookEntity).Name} not found.");
    }
}