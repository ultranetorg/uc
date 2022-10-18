namespace UC.Net.Node.MAUI.ViewModels.Account;

public partial class ManageAccountsViewModel : BaseTransactionsViewModel
{
	private readonly IServicesMockData _service;

	[ObservableProperty]
    private Wallet _selectedItem;

	[ObservableProperty]
    private CustomCollection<Wallet> _wallets = new();

    public ManageAccountsViewModel(IServicesMockData service, ILogger<ManageAccountsViewModel> logger) : base(logger)
    {
		_service = service;
		LoadData();
    }
	
	private void LoadData()
	{
		Wallets.Clear();
		Wallets.AddRange(_service.Wallets);
	}
}
