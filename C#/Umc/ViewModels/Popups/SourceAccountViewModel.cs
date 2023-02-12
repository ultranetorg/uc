namespace UC.Umc.ViewModels.Popups;

public partial class SourceAccountViewModel : BaseViewModel
{
	private readonly IAccountsService _service;

	 // selected account
	[ObservableProperty]
    private AccountViewModel _account;

	[ObservableProperty]
    private CustomCollection<AccountViewModel> _accounts = new();

	[ObservableProperty]
    private string _filter;

	public bool AllAccountsEnabled { get; set; }

    public SourceAccountViewModel(IAccountsService service, ILogger<SourceAccountViewModel> logger) : base(logger)
    {
		_service = service;
		LoadData();
    }
	
	[RelayCommand]
    public async Task FilterAccountsAsync()
    {
		try
		{
			Guard.IsNotNull(Filter);
			InitializeLoading();

			// Search accounts
			var filteredList = await _service.ListAccountsAsync(Filter, AllAccountsEnabled);
			Accounts.Clear();
			Accounts.AddRange(filteredList);

			if (Account != null)
			{
				foreach (var item in Accounts)
				{
					item.IsSelected = false;
				}
				Account = null;
			}
			
			FinishLoading();
		}
		catch (Exception ex)
		{
			ToastHelper.ShowErrorMessage(_logger);
			_logger.LogError("FilterAccountsAsync Error: {Message}", ex.Message);
		}
    }

	[RelayCommand]
    private void ItemTapped(AccountViewModel account)
    {
		try
		{
			foreach (var item in Accounts)
			{
				item.IsSelected = false;
			}
			account.IsSelected = true;
			Account = account;
		}
		catch (Exception ex)
		{
			ToastHelper.ShowErrorMessage(_logger);
			_logger.LogError("ItemTapped Error: {Message}", ex.Message);
		}
	}

	[RelayCommand]
    private void Close() => ClosePopup();
	
	private void LoadData()
	{
		Accounts = new(_service.ListAllAccounts());
	}

	public void AddAllOption()
	{
		AllAccountsEnabled = true;
		Accounts = new(Accounts.Prepend(DefaultDataMock.AllAccountOption));
		Account = DefaultDataMock.AllAccountOption;
	}
}
