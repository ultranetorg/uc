namespace UC.Net.Node.MAUI.Services;

public interface IServicesMockData
{
    public IList<AccountViewModel> Accounts { get; }
    public IList<Author> Authors { get; }
    public IList<Product> Products { get; }
    public IList<TransactionViewModel> Transactions { get; }
    public IList<AccountColor> AccountColors { get; }
    public IList<Models.Emission> Emissions { get; }
    public IList<Notification> Notifications { get; }
}
