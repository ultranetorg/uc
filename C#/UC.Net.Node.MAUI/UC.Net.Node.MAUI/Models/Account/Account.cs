namespace UC.Net.Node.MAUI.Models;

public class Account
{
    public Account(string address)
    {
        Address = address;
    }

    public Guid Id { get; set; }

    public string Name { get; set; }

    public string Address { get; private set; }

    public decimal Balance { get; set; }

    public GradientBrush Color { get; set; }

    public bool ShowOnDashboard { get; set; }

    public IList<Author> Authors { get; set; } = new List<Author>();

    public IList<Transaction> Transactions { get; set; } = new List<Transaction>();

	// lets say 1 unts = $1 unless we can recieve rate
	public string DisplayAmount => $"{Math.Round(Balance)} UNT (${Math.Round(Balance)})";

	public string IconCode => Address?[..4];
}