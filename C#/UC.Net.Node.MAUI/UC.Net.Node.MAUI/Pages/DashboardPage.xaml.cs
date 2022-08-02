namespace UC.Net.Node.MAUI.Pages;

public partial class DashboardPage : CustomPage
{
    public DashboardPage()
    {
        InitializeComponent();
        BindingContext = new DashboardViewModel(ServiceHelper.GetService<ILogger<DashboardViewModel>>());
    }
}
public class DashboardViewModel : BaseViewModel
{
    public DashboardViewModel(ILogger<DashboardViewModel> logger) : base(logger)
    {
        Wallets.Add(new Wallet
        {
            Id = Guid.NewGuid(),
            Unts = 5005,
            IconCode = "47F0",
            Name = "Main ultranet"
        });
        Wallets.Add(new Wallet
        {
            Id = Guid.NewGuid(),
            Unts = 5005,
            IconCode = "2T52",
            Name = "Main ultranet"
        });
        Wallets.Add(new Wallet
        {
            Id = Guid.NewGuid(),
            Unts = 5005,
            IconCode = "9MDL",
            Name = "Main ultranet"
        });

        Transactions.Add(new Transaction
        {
            FromId = Generator.GenerateUniqueID(6),
            ToId = Generator.GenerateUniqueID(6),
            Unt = 540,
            Name = "UNT Transfer",
            Status = TransactionsStatus.Pending
        });
        Transactions.Add(new Transaction
        {
            FromId = Generator.GenerateUniqueID(6),
            ToId = Generator.GenerateUniqueID(6),
            Unt = 590,
            Name = "UNT Transfer",
            Status = TransactionsStatus.Sent
        });
        Transactions.Add(new Transaction
        {
            FromId = Generator.GenerateUniqueID(6),
            ToId = Generator.GenerateUniqueID(6),
            Unt = 590,
            Name = "UNT Transfer",
            Status = TransactionsStatus.Failed
        });
    }

    public Command AuthorsCommand
    {
        get => new Command(AuthorsExcute);
    }
    public async void AuthorsExcute()
    {
        await Shell.Current.Navigation.PushAsync(new AuthorsPage());
    }
    public Command ProductsCommand
    {
        get => new Command(ProductsExcute);
    }
    public async void ProductsExcute()
    {
        await Shell.Current.Navigation.PushAsync(new ProductsPage());
    }
    public Command ETHTransferCommand
    {
        get => new Command(ETHTransferExcute);
    }
    public async void ETHTransferExcute()
    {
        await Shell.Current.Navigation.PushAsync(new ETHTransferPage());
    }
    public Command TransactionsCommand
    {
        get => new Command(TransactionsCommandExcute);
    }
    public async void TransactionsCommandExcute()
    {
        await Shell.Current.Navigation.PushAsync(new TransactionsPage());
    }
    public Command AccountsCommand
    {
        get => new Command(AccountsCommandExcute);
    }
    public async void AccountsCommandExcute()
    {
        await Shell.Current.Navigation.PushAsync(new ManageAccountsPage());
    }
    CustomCollection<Wallet> _Wallets =new CustomCollection<Wallet>();
    public CustomCollection<Wallet>  Wallets
    {
        get { return _Wallets; }
        set { SetProperty(ref _Wallets, value); }
    }
    CustomCollection<Transaction> _Transactions = new CustomCollection<Transaction>();
    public CustomCollection<Transaction> Transactions
    {
        get { return _Transactions; }
        set { SetProperty(ref _Transactions, value); }
    }
}
