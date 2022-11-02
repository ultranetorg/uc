using UC.Net.Node.MAUI.Constants;

namespace UC.Net.Node.MAUI.ViewModels;

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
    private async Task CreateAsync() => await Navigation.GoToUpwardsAsync(ShellBaseRoutes.CREATE_ACCOUNT);

	[RelayCommand]
    private async Task RestoreAsync() => await Navigation.GoToUpwardsAsync(ShellBaseRoutes.RESTORE_ACCOUNT);
}
