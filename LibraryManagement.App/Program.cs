using LibraryManagement.Core.Entities;
using LibraryManagement.Core.Enums;
using LibraryManagement.DataAccess.Interfaces;
using LibraryManagement.DataAccess.Repositories;
using LibraryManagement.Services.Auth;

BookEntity bookEntity = new BookEntity
{
    Author = "John Doe Updated",
    Id = 1,
    Isbn = "978 - 2",
    Quantity = 5,
    Title = "Title",
};


AuthService authService = new AuthService(new UserRepository());