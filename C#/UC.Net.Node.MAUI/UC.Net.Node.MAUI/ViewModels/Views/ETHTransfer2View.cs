namespace UC.Net.Node.MAUI.ViewModels;

public partial class ETHTransfer2ViewModel : BaseViewModel
{
	private readonly IServicesMockData _service;

	[ObservableProperty]
    private Wallet _wallet = DefaultDataMock.Wallet1;

	[ObservableProperty]
	private CustomCollection<Wallet> _wallets = new();

	[ObservableProperty]
	private AccountColor _selectedAccountColor;

	[ObservableProperty]
	private bool _accountsShown;

	public ETHTransfer2ViewModel(IServicesMockData service, ILogger<ETHTransfer2ViewModel> logger): base(logger)
	{
		_service = service;
		LoadData();
	}

	[RelayCommand]
	private void ItemTapped(Wallet wallet)
	{
		foreach (var item in Wallets)
		{
			item.IsSelected = false;
		}
		wallet.IsSelected = true;
	}

	[RelayCommand]
	private void ShowHideAccounts()
	{
		AccountsShown = !AccountsShown;
	}
	
	private void LoadData()
	{
		Wallets.Clear();
		Wallets.AddRange(_service.Wallets);
	}
}
