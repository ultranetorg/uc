namespace UC.Net.Node.MAUI.Models;

public class Account
{
    public Account(string address)
    {
        Address = address;
    }

    public string Name { get; set; }

    public string Address { get; private set; }

    public decimal Balance { get; set; }

    public GradientColor Color { get; set; }

    public bool ShowOnDashboard { get; set; }

    public IList<Author> Authors { get; set; } = new List<Author>();

    public IList<Wallet> Wallets { get; set; } = new List<Wallet>();

    public IList<Transaction> Transactions { get; set; } = new List<Transaction>();
}