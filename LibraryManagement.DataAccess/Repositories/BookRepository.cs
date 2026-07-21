using LibraryManagement.Core.Entities;
using LibraryManagement.Core.Exceptions;
using LibraryManagement.Core.Interfaces;
namespace LibraryManagement.DataAccess.Repositories;

public class BookRepository : BaseRepository<BookEntity>, IBookRepository
{
    private static readonly string DefaultPath = DatabasePathResolver.Resolve("Books.json");

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
            throw new NotFoundException($"{typeof(BookEntity).Name} with ISBN '{isbn}' was not found.");
    }

    public BookEntity? FindBookByIsbn(string isbn)
    {
        var books = ReadEntitiesFromFile();
        return books.FirstOrDefault(b => b.Isbn == isbn);
    }
}