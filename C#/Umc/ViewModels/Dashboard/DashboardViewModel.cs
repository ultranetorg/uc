namespace UC.Umc.ViewModels;

public partial class DashboardViewModel : BaseViewModel
{
	private readonly IAccountsService _accountsService;
	private readonly IAuthorsService _authorsService;
	private readonly IProductsService _productService;
	private readonly ITransactionsService _transactionsService;

	[ObservableProperty]
    private CustomCollection<AccountViewModel> _accounts = new();

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(AuctionsOutbidded))]
	[NotifyPropertyChangedFor(nameof(AuthorsRenewal))]
    private CustomCollection<AuthorViewModel> _authors = new();

	[ObservableProperty]
    private CustomCollection<TransactionViewModel> _transactions = new();

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(NumberOfProducts))]
    private CustomCollection<ProductViewModel> _products = new();
	
	[ObservableProperty]
	public int _numberOfAccounts = 0;

	public int NumberOfProducts => Products?.Count ?? 0;
	public string AuctionsOutbidded => $"{Authors?.Where(x => x.Status == AuthorStatus.Auction).Count() ?? 0} " +
		$"({Authors?.Where(x => x.Status == AuthorStatus.Auction && x.BidStatus == BidStatus.Higher).Count() ?? 0})";
	public string AuthorsRenewal => $"{Authors?.Count ?? 0} ({Authors?.Where(x => x.ExpiresSoon).Count() ?? 0})";

    public DashboardViewModel(IAccountsService accountsService, IAuthorsService authorsService, IProductsService productService,
		ITransactionsService transactionsService, ILogger<DashboardViewModel> logger) : base(logger)
    {
		_accountsService = accountsService;
		_authorsService = authorsService;
		_productService = productService;
		_transactionsService = transactionsService;
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
		var accounts = await _accountsService.ListAccountsAsync();
		var authors = await _authorsService.GetAccountAuthorsAsync();
		var products = await _productService.GetAllProductsAsync();
		var transactions = await _transactionsService.ListTransactionsAsync(null, null, 3);

		Accounts = new(accounts.Count < 3 ? accounts : accounts.Take(3));
		Authors = new(authors);
		Transactions = new(transactions);
		Products = new(products);
		NumberOfAccounts = accounts.Count;
	}
}
