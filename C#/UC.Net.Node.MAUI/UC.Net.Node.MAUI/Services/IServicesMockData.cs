namespace UC.Net.Node.MAUI.Services;

public interface IServicesMockData
{
    public IList<Account> Accounts { get; set; }
    public IList<Author> Authors { get; }
    public IList<Product> Products { get; }
    public IEnumerable<Transaction> Transactions { get; }
    public IList<AccountColor> AccountColors { get; }
    public IList<Emission> Emissions { get; }
    public IList<Notification> Notifications { get; }
}
