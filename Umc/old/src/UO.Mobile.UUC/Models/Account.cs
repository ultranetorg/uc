using UO.Mobile.UUC.Models.Transactions;

namespace UO.Mobile.UUC.Models;

public class Account
{
    public Account(string address)
    {
        Guard.Against.NullOrEmpty(address, nameof(address));

        Address = address;
    }

    public string Name { get; set; }

    public string Address { get; private set; }

    public decimal Balance { get; set; }

    public GradientColor Color { get; set; }

    public bool ShowOnDashboard { get; set; }

    public IList<Author> Authors { get; set; } = new List<Author>();

    public IList<Transaction> Transactions { get; set; } = new List<Transaction>();
}
