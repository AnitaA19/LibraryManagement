namespace LibraryManagement.Core.Entities;

public class BookEntity : BaseEntity
{
    public string Isbn { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public int Quantity { get; set; }
}
