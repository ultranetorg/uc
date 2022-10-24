namespace UC.Net.Node.MAUI.ViewModels;

public partial class ManageAccountsViewModel : BaseTransactionsViewModel
{
	private readonly IServicesMockData _service;

	[ObservableProperty]
    private Wallet _selectedItem;

	[ObservableProperty]
    private CustomCollection<Account> _accounts = new();

    public ManageAccountsViewModel(IServicesMockData service, ILogger<ManageAccountsViewModel> logger) : base(logger)
    {
		_service = service;
		LoadData();
    }
	
	private void LoadData()
	{
		Accounts.Clear();
		Accounts.AddRange(_service.Accounts);
	}
}
