namespace UC.Umc.ViewModels.Popups;

public partial class RecipientAccountViewModel : BaseViewModel
{
	private readonly IAccountsService _service;

	[ObservableProperty]
    private CustomCollection<AccountViewModel> _accounts = new();
        
	[ObservableProperty]
    private AccountViewModel _account;

	[ObservableProperty]
	private bool _isLocal = true;

	[ObservableProperty]
	private bool _isExternal;

	[ObservableProperty]
	private bool _isQrCode;

	[ObservableProperty]
    private string _filter;

	[ObservableProperty]
	private string externalAccount;

    public RecipientAccountViewModel(IAccountsService service, ILogger<RecipientAccountViewModel> logger): base(logger)
    {
		_service = service;
		LoadData();
    }

	[RelayCommand]
    private void ChangeToLocalSource()
	{
		IsLocal = true;
		IsExternal = false;
		IsQrCode = false;
	}

	[RelayCommand]
    private void ChangeToExternalSource()
	{
		IsLocal = false;
		IsExternal = true;
		IsQrCode = false;
	}

	[RelayCommand]
    private void ChangeToQrCodeSource()
	{
		IsLocal = false;
		IsExternal = false;
		IsQrCode = true;
	}
	
	[RelayCommand]
    public async Task FilterAccountsAsync()
    {
		try
		{
			Guard.IsNotNull(Filter);
			InitializeLoading();

			// Search accounts
			var filteredList = await _service.ListAccountsAsync(Filter);
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
    private void Close() => ClosePopup();

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
	
	private void LoadData()
	{
		Accounts = new(_service.ListAllAccounts());
	}
}
