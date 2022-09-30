namespace UC.Net.Node.MAUI.ViewModels.Pages;

public partial class DashboardViewModel : BaseViewModel
{
	private readonly ITransactionsService _transactionsService;
	private readonly IWalletsService _walletsService;

	[ObservableProperty]
    private CustomCollection<Wallet> _wallets = new();

	[ObservableProperty]
    private CustomCollection<Transaction> _transactions = new();

    public DashboardViewModel(ITransactionsService transactionsService, IWalletsService walletsService, ILogger<DashboardViewModel> logger) : base(logger)
    {
		_transactionsService = transactionsService;
		_walletsService = walletsService;
    }

	[RelayCommand]
    public async void AuthorsExcuteAsync()
    {
        await Shell.Current.Navigation.PushAsync(new AuthorsPage());
    }

	[RelayCommand]
    public async void ProductsExcuteAsync()
    {
        await Shell.Current.Navigation.PushAsync(new ProductsPage());
    }
	
	[RelayCommand]
    public async void ETHTransferExcuteAsync()
    {
        await Shell.Current.Navigation.PushAsync(new ETHTransferPage());
    }
	
	[RelayCommand]
    public async void TransactionsExcuteAsync()
    {
        await Shell.Current.Navigation.PushAsync(new TransactionsPage());
    }

	[RelayCommand]
    public async void AccountsExcuteAsync()
    {
        await Shell.Current.Navigation.PushAsync(new ManageAccountsPage());
    }

	internal async Task InitializeAsync()
	{
		Transactions.Clear();
		Wallets.Clear();

		var transactions = await _transactionsService.GetLastAsync(5);
		var wallets = await _walletsService.GetAllAsync();

		Transactions.AddRange(transactions);
		Wallets.AddRange(wallets.Count < 3 ? wallets : wallets.Take(3));

		await Task.Delay(10);
	}
}
