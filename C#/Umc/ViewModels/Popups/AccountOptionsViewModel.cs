namespace UC.Umc.ViewModels.Popups;

public partial class AccountOptionsViewModel : BaseViewModel
{
	[ObservableProperty]
    public AccountViewModel _account;

	public AccountOptionsViewModel(ILogger<AccountOptionsViewModel> logger) : base(logger)
	{
	}

	[RelayCommand]
	private async Task SendAsync()
	{
		// TODO: add account parameter
		await Navigation.GoToAsync(nameof(SendPage));
		ClosePopup();
	}

	[RelayCommand]
	private async Task ShowPrivateKeyAsync(AccountViewModel account)
	{
		await Navigation.GoToAsync(nameof(PrivateKeyPage),
			new Dictionary<string, object>()
		{
			{ nameof(AccountViewModel), account }
		});
		ClosePopup();
	}

	[RelayCommand]
	private async Task DeleteAsync(AccountViewModel account)
	{
		await Navigation.GoToAsync(nameof(DeleteAccountPage),
			new Dictionary<string, object>()
		{
			{ nameof(AccountViewModel), account }
		});
		ClosePopup();
	}
}
