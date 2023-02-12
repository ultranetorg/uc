namespace UC.Umc.ViewModels;

public partial class ManageAccountsViewModel : BaseViewModel
{
	private readonly IAccountsService _service;

	[ObservableProperty]
    private CustomCollection<AccountViewModel> _accounts = new();

    public ManageAccountsViewModel(IAccountsService service, ILogger<ManageAccountsViewModel> logger) : base(logger)
    {
		_service = service;
    }
	
	public async Task InitializeAsync()
	{
		var accounts = await _service.ListAccountsAsync();
		Accounts = new(accounts);
	}

	[RelayCommand]
    private async Task OpenOptionsAsync(AccountViewModel account)
	{
		try
		{
			Guard.IsNotNull(account);

			await ShowPopup(new AccountOptionsPopup(account));
		}
		catch(ArgumentException ex)
		{
			_logger.LogError("OpenOptionsAsync: Account cannot be null, Error: {Message}", ex.Message);
		}
		catch (Exception ex)
		{
			_logger.LogError("OpenOptionsAsync Error: {Message}", ex.Message);
		}
	}

	[RelayCommand]
    private async Task OpenDetailsAsync(AccountViewModel account) => 
		await Navigation.GoToAsync(ShellBaseRoutes.ACCOUNT_DETAILS, new Dictionary<string,object>()
		{
			{ QueryKeys.ACCOUNT, account }
		});

	[RelayCommand]
    private async Task ReceiveAsync(AccountViewModel account) =>
		await Navigation.GoToAsync(nameof(SendPage),
			new Dictionary<string, object>()
		{
			{ QueryKeys.SOURCE_ACCOUNT, null },
			{ QueryKeys.RECIPIENT_ACCOUNT, account }
		});
	
	[RelayCommand]
    private async Task SendAsync(AccountViewModel account) =>
		await Navigation.GoToAsync(nameof(SendPage),
			new Dictionary<string, object>()
		{
			{ QueryKeys.SOURCE_ACCOUNT, account },
			{ QueryKeys.RECIPIENT_ACCOUNT, null }
		});

	[RelayCommand]
    private async Task CreateAsync() => await Navigation.GoToAsync(ShellBaseRoutes.CREATE_ACCOUNT);

	[RelayCommand]
    private async Task RestoreAsync() => await Navigation.GoToAsync(ShellBaseRoutes.RESTORE_ACCOUNT);
}
