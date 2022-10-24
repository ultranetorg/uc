namespace UC.Net.Node.MAUI.ViewModels;

public partial class ETHTransfer2ViewModel : BaseViewModel
{
	private readonly IServicesMockData _service;

	[ObservableProperty]
    private Account _account = DefaultDataMock.Account1;

	[ObservableProperty]
	private CustomCollection<Account> _accounts = new();

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
	private void ItemTapped(Account account)
	{
		//foreach (var item in Accounts)
		//{
		//	item.IsSelected = false;
		//}
		//wallet.IsSelected = true;
	}

	[RelayCommand]
	private void ShowHideAccounts()
	{
		AccountsShown = !AccountsShown;
	}
	
	private void LoadData()
	{
		Accounts.Clear();
		Accounts.AddRange(_service.Accounts);
	}
}
