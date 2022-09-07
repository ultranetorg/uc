namespace UC.Net.Node.MAUI.ViewModels.Pages;

public partial class DashboardViewModel : BaseViewModel
{
	private readonly ITransactionsService _service;

	[ObservableProperty]
    private CustomCollection<Wallet> _wallets = new();

	[ObservableProperty]
    private CustomCollection<Transaction> _transactions = new();

    public DashboardViewModel(ITransactionsService service, ILogger<DashboardViewModel> logger) : base(logger)
    {
		_service = service;
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
    public async void TransactionsCommandExcuteAsync()
    {
        await Shell.Current.Navigation.PushAsync(new TransactionsPage());
    }

	[RelayCommand]
    public async void AccountsCommandExcuteAsync()
    {
        await Shell.Current.Navigation.PushAsync(new ManageAccountsPage());
    }

	internal async Task InitializeAsync()
	{
		var transactions = await _service.GetLastAsync(20);

		Transactions.AddRange(transactions);
	}
}
