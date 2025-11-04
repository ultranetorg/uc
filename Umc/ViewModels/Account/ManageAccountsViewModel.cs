namespace UC.Umc.ViewModels;

public partial class ManageAccountsViewModel : BasePageViewModel
{
	private readonly IAccountsService _service;

	[ObservableProperty]
    private CustomCollection<AccountViewModel> _accounts = new();

    public ManageAccountsViewModel(INotificationsService notificationService, IAccountsService service,
		ILogger<ManageAccountsViewModel> logger) : base(notificationService, logger)
    {
		_service = service;
    }
	
	public async Task InitializeAsync()
	{
		var accounts = await _service.ListAccountsAsync();
		Accounts = new(accounts);
	}

	[RelayCommand]
    private async Task OpenDetailsAsync(AccountViewModel account) => 
		await Navigation.GoToAsync(Routes.ACCOUNT_DETAILS, new Dictionary<string,object>()
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
    private async Task CreateAsync() => await Navigation.GoToAsync(Routes.CREATE_ACCOUNT);

	[RelayCommand]
    private async Task RestoreAsync() => await Navigation.GoToAsync(Routes.RESTORE_ACCOUNT);
}
