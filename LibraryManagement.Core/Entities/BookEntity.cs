namespace LibraryManagement.Core.Entities;

public class BookEntity : BaseEntity
{
    public string Isbn { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }

    private int _quantity;

    public int Quantity
    {
        get => _quantity;
        set
        {
            if (value < 0)
            {
                throw new ArgumentException("Quantity cannot be negative.");
            }

            _quantity = value;
        }
    }
}