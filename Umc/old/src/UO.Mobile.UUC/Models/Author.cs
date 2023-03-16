namespace UO.Mobile.UUC.Models;

public class Author
{
    public string Name { get; set; }

    public string Title { get; set; }

    public Account Account { get; set; }

    public IList<Product> Products { get; set; } = new List<Product>();
}
