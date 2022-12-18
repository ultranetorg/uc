namespace UC.Umc.Services;

public interface IServicesMockData
{
    public IList<AccountViewModel> Accounts { get; }
    public IList<AuthorViewModel> Authors { get; }
    public IList<ProductViewModel> Products { get; }
    public IList<TransactionViewModel> Transactions { get; }
    public IList<AccountColor> AccountColors { get; }
    public IList<Emission> Emissions { get; }
    public IList<Notification> Notifications { get; }
}
