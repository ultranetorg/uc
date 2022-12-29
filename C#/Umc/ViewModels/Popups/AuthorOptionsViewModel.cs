namespace UC.Umc.ViewModels.Popups;

public partial class AuthorOptionsViewModel : BaseViewModel
{
	[ObservableProperty]
    public AuthorViewModel _author;

	public AuthorOptionsViewModel(ILogger<AuthorOptionsViewModel> logger) : base(logger)
	{
	}

	[RelayCommand]
	private async Task ShowPrivateKeyAsync()
	{
		await Navigation.GoToAsync(nameof(PrivateKeyPage),
			new Dictionary<string, object>()
		{
			{ QueryKeys.AUTHOR, Author }
		});
		ClosePopup();
	}

	[RelayCommand]
	private async Task DeleteAsync()
	{
		await Navigation.GoToAsync(nameof(DeleteAccountPage),
			new Dictionary<string, object>()
		{
			{ QueryKeys.AUTHOR, Author }
		});
		ClosePopup();
	}

	[RelayCommand]
	private async Task BackupAsync()
	{
		// TODO
		await Task.Delay(1);
	}

	[RelayCommand]
	private async Task HideFromDashboardAsync()
	{
		// TODO
		await Task.Delay(1);
	}
}
