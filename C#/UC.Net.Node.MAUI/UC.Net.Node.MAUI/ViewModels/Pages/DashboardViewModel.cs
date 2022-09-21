namespace UC.Net.Node.MAUI.ViewModels.Pages;

public partial class DashboardViewModel : BaseViewModel
{
	private readonly IServicesMockData _service;

	[ObservableProperty]
    private CustomCollection<Wallet> _wallets = new();

	[ObservableProperty]
    private CustomCollection<Transaction> _transactions = new();

    public DashboardViewModel(IServicesMockData service, ILogger<DashboardViewModel> logger) : base(logger)
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
		Transactions.AddRange(_service.Transactions);
		Wallets.AddRange(_service.Wallets);

		await Task.Delay(10);
	}
}
