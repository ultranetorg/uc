using UC.Umc.Constants;

namespace UC.Umc.ViewModels;

public partial class ManageAccountsViewModel : BaseAccountViewModel
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
		Accounts.Clear();
		var accounts = await _service.GetAllAsync();
		Accounts.AddRange(accounts);
	}

	[RelayCommand]
    private async Task OpenOptionsAsync(AccountViewModel account)
    {
        if (account == null) return;
        await AccountOptionsPopup.Show(account);
    }

	[RelayCommand]
    private async Task OpenDetailsAsync(AccountViewModel account) => 
		await NavigateToAsync(ShellBaseRoutes.ACCOUNT_DETAILS, new Dictionary<string,object>()
		{
			// todo: move query key to constants
			{ nameof(AccountDetailsPage), account }
		});

	[RelayCommand]
    private async Task ReceiveAsync(AccountViewModel account)
    {
		// TODO
		await Task.Delay(1);
    }

	[RelayCommand]
    private async Task SendAsync(AccountViewModel account)
    {
		// TODO
		await Task.Delay(1);
    }

	[RelayCommand]
    private async Task CreateAsync() => await NavigateToAsync(ShellBaseRoutes.CREATE_ACCOUNT);

	[RelayCommand]
    private async Task RestoreAsync() => await NavigateToAsync(ShellBaseRoutes.RESTORE_ACCOUNT);
}
