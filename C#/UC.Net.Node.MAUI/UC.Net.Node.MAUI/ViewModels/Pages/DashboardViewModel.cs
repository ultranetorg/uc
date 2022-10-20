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

	// TODO: replace AccountDetails with ManageAccount Page

	[RelayCommand]
    public async Task AccountsExcuteAsync() => await Navigation.GoToUpwardsAsync(nameof(AccountDetailsPage));

	[RelayCommand]
    public async Task AuthorsExcuteAsync() => await Navigation.GoToUpwardsAsync(nameof(AuthorsPage));

	[RelayCommand]
    public async Task ProductsExcuteAsync() => await Navigation.GoToUpwardsAsync(nameof(ProductsPage));
	
	[RelayCommand]
    public async Task ETHTransferExcuteAsync() => await Navigation.GoToUpwardsAsync(nameof(ETHTransferPage));
	
	[RelayCommand]
    public async Task TransactionsExcuteAsync() => await Navigation.GoToUpwardsAsync(nameof(TransactionsPage));
	
	[RelayCommand]
    public async Task NetworkExcuteAsync() => await Navigation.GoToUpwardsAsync(nameof(NetworkPage));
	
	[RelayCommand]
    public async Task HelpExcuteAsync() => await Navigation.GoToUpwardsAsync(nameof(HelpPage));

	internal async Task InitializeAsync()
	{
		Wallets.Clear();
		Transactions.Clear();

		var wallets = await _walletsService.GetAllAsync();
		var transactions = await _transactionsService.GetLastAsync(3);

		Wallets.AddRange(wallets.Count < 3 ? wallets : wallets.Take(3));
		Transactions.AddRange(transactions);
	}
}
