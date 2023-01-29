namespace UC.Umc.ViewModels;

public partial class DashboardViewModel : BaseViewModel
{
	private readonly ITransactionsService _transactionsService;
	private readonly IAccountsService _accountsService;

	[ObservableProperty]
    private CustomCollection<AccountViewModel> _accounts = new();

	[ObservableProperty]
    private CustomCollection<TransactionViewModel> _transactions = new();

    public DashboardViewModel(ITransactionsService transactionsService, IAccountsService accountsService, ILogger<DashboardViewModel> logger) : base(logger)
    {
		_transactionsService = transactionsService;
		_accountsService = accountsService;
    }

	[RelayCommand]
    public async Task AccountsExcuteAsync() => await Navigation.GoToUpwardsAsync(ShellBaseRoutes.ACCOUNTS);

	[RelayCommand]
    public async Task AuthorsExcuteAsync() => await Navigation.GoToUpwardsAsync(ShellBaseRoutes.AUTHORS);

	[RelayCommand]
    public async Task ProductsExcuteAsync() => await Navigation.GoToUpwardsAsync(ShellBaseRoutes.PRODUCTS);

	[RelayCommand]
    public async Task SearchExcuteAsync() => await Navigation.GoToUpwardsAsync(ShellBaseRoutes.PRODUCT_SEARCH);

	[RelayCommand]
	public async Task TransactionsExcuteAsync() => await Navigation.GoToUpwardsAsync(ShellBaseRoutes.TRANSACTIONS);

	[RelayCommand]
    public async Task ETHTransferExcuteAsync() => await Navigation.GoToUpwardsAsync(ShellBaseRoutes.TRANSFER);
	
	[RelayCommand]
    public async Task NetworkExcuteAsync() => await Navigation.GoToUpwardsAsync(ShellBaseRoutes.NETWORK);
	
	[RelayCommand]
    public async Task HelpExcuteAsync() => await Navigation.GoToUpwardsAsync(ShellBaseRoutes.HELP);

	internal async Task InitializeAsync()
	{
		Accounts.Clear();
		Transactions.Clear();

		var accounts = await _accountsService.GetAllAsync();
		var transactions = await _transactionsService.GetLastAsync(3);

		Accounts.AddRange(accounts.Count < 3 ? accounts : accounts.Take(3));
		Transactions.AddRange(transactions);
	}
}
