namespace UC.Umc.ViewModels.Popups;

public partial class SourceAccountViewModel : BaseViewModel
{
	private readonly IServicesMockData _service;

	 // selected account
	[ObservableProperty]
    private AccountViewModel _account;

	[ObservableProperty]
    private CustomCollection<AccountViewModel> _accounts = new();

	public bool AllAccountsEnabled { get; set; }

    public SourceAccountViewModel(IServicesMockData service, ILogger<SourceAccountViewModel> logger) : base(logger)
    {
		_service = service;
		LoadData();
    }

	[RelayCommand]
    private void ItemTapped(AccountViewModel account)
    {
		foreach (var item in Accounts)
		{
			item.IsSelected = false;
		}
		account.IsSelected = true;
		Account = account;
	}

	[RelayCommand]
    private void Close() => ClosePopup();
	
	private void LoadData()
	{
		Accounts = new(_service.Accounts);
	}

	public void AddAllOption()
	{
		AllAccountsEnabled = true;
		Accounts = new(Accounts.Prepend(DefaultDataMock.AllAccountOption));
		Account = DefaultDataMock.AllAccountOption;
	}
}
