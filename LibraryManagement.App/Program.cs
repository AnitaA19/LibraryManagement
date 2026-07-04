using LibraryManagement.Core.Entities;
using LibraryManagement.Core.Enums;
using LibraryManagement.DataAccess.Repositories;

string path = @"C:\Users\balas\source\repos\LibraryManagement\LibraryManagement.DataAccess\Database\Books.json";

BookEntity bookEntity = new BookEntity
{
    Author = "John Doe Updated",
    Id = 1,
    Isbn = "978 - 2",
    Quantity = 5,
    Title = "Title",
};

BaseRepository<BookEntity> bookRepository = new BaseRepository<BookEntity>(path);
bookRepository.UpdateEntity(bookEntity);

