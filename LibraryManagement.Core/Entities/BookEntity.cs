using LibraryManagement.Core.Exceptions;
using System.Text.Json.Serialization;

namespace LibraryManagement.Core.Entities;

public class BookEntity : BaseEntity
{
    public string Isbn { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }

    private int _quantity;

    [JsonInclude]
    public int Quantity
    {
        get => _quantity;
        private set
        {
            if (value < 0)
            {
                throw new ValidationException("Quantity cannot be negative.");
            }

            _quantity = value;
        }
    }

    public BookEntity()
    {
    }

    public BookEntity(string isbn, string title, string author, int quantity)
    {
        Isbn = isbn;
        Title = title;
        Author = author;
        Quantity = quantity;
    }

    public void IncreaseQuantity(int amount)
    {
        if (amount <= 0)
        {
            throw new ValidationException("Amount to add must be positive.");
        }

        Quantity += amount;
    }

    public void DecreaseQuantity(int amount)
    {
        if (amount <= 0)
        {
            throw new ValidationException("Amount to remove must be positive.");
        }

        if (amount > Quantity)
        {
            throw new ValidationException("Cannot remove more than the available quantity.");
        }

        Quantity -= amount;
    }
}