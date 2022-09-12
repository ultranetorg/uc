namespace UC.Net.Node.MAUI.Models;

public class Author
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Title { get; set; }
    public int CurrentBid { get; set; }
    public BidStatus BidStatus { get; set; }
    public string ActiveDue { get; internal set; }
    public Account Account { get; set; }
    public IList<Product> Products { get; set; } = new List<Product>();
}

public enum BidStatus
{
    None, Higher, Lower
}
