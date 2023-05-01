namespace UC.Umc.ViewModels;

public partial class RestoreAccountViewModel : BaseAccountViewModel
{
	[ObservableProperty]
	private bool _isPrivateKey = true;

	[ObservableProperty]
	private bool _isFilePath;

	[ObservableProperty]
	private bool _showFilePassword;

	[ObservableProperty]
	private string _privateKey;

	[ObservableProperty]
	private string _walletPath;

	[ObservableProperty]
	private string _walletPassword;

    public RestoreAccountViewModel(INotificationsService notificationService, ILogger<RestoreAccountViewModel> logger) : base(notificationService, logger)
    {
    }

	[RelayCommand]
	private void ChangeKeySource()
	{
		IsPrivateKey = !IsPrivateKey;
		IsFilePath = !IsFilePath;
	}

	[RelayCommand]
    private async Task OpenFilePickerAsync()
	{
		try
		{
			WalletPath = await CommonHelper.GetPathToWalletAsync();
			ShowFilePassword = !string.IsNullOrEmpty(WalletPath);
		}
		catch (Exception ex)
		{
			_logger.LogError("OpenFilePickerAsync Error: {Message}", ex.Message);
		}
	}

	[RelayCommand]
	private async Task NextWorkaroundAsync()
	{
		try
		{
			var isValidStep = (IsPrivateKey && !string.IsNullOrEmpty(PrivateKey))
				|| (IsFilePath && !string.IsNullOrEmpty(WalletPath) && !string.IsNullOrEmpty(WalletPassword));

			if (Position == 0 && isValidStep)
			{
				// Workaround for this bug: https://github.com/dotnet/maui/issues/9749
				Position = 1;
				Position = 0;
				Position = 1;
			}
			else if (Position == 1)
			{
				await Navigation.PopAsync();
				await ToastHelper.ShowMessageAsync("Successfully restored!");
			}
		}
		catch (Exception ex)
		{
			_logger.LogError("NextWorkaroundAsync Error: {Message}", ex.Message);
		}
	}
}
