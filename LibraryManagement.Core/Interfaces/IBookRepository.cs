using LibraryManagement.Core.Entities;

namespace LibraryManagement.Core.Interfaces;

public interface IBookRepository : IBaseRepository<BookEntity>
{
    BookEntity GetBookByIsbn(string isbn);
    BookEntity? FindBookByIsbn(string isbn);
}
