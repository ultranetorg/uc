namespace UC.Umc.ViewModels;

public partial class ETHTransfer2ViewModel : BaseViewModel
{
	private readonly IServicesMockData _service;

	[ObservableProperty]
    private AccountViewModel _account;

	[ObservableProperty]
	private CustomCollection<AccountViewModel> _accounts = new();

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
	private void ItemTapped(AccountViewModel account)
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
