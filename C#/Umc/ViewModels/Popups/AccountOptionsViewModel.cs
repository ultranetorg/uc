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
		await Navigation.GoToAsync(nameof(SendPage),
			new Dictionary<string, object>()
		{
			{ QueryKeys.SOURCE_ACCOUNT, Account },
			{ QueryKeys.RECIPIENT_ACCOUNT, null }
		});
		ClosePopup();
	}

	[RelayCommand]
	private async Task ReceiveAsync()
	{
		await Navigation.GoToAsync(nameof(SendPage),
			new Dictionary<string, object>()
		{
			{ QueryKeys.SOURCE_ACCOUNT, null },
			{ QueryKeys.RECIPIENT_ACCOUNT, Account }
		});
		ClosePopup();
	}

	[RelayCommand]
	private async Task ShowPrivateKeyAsync()
	{
		await Navigation.GoToAsync(nameof(PrivateKeyPage),
			new Dictionary<string, object>()
		{
			{ QueryKeys.ACCOUNT, Account }
		});
		ClosePopup();
	}

	[RelayCommand]
	private async Task DeleteAsync()
	{
		await Navigation.GoToAsync(nameof(DeleteAccountPage),
			new Dictionary<string, object>()
		{
			{ QueryKeys.ACCOUNT, Account }
		});
		ClosePopup();
	}
}
