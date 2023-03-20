using System.ComponentModel.DataAnnotations;

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
	private string _walletFilePath;

	[ObservableProperty]
	private string _walletFilePassword;

    [ObservableProperty]
    [NotifyDataErrorInfo]
    [Required(ErrorMessage = "Required")]
    [NotifyPropertyChangedFor(nameof(AccountNameError))]
    private string _accountName;

    public string AccountNameError => GetControlErrorMessage(nameof(AccountName));

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
			WalletFilePath = await CommonHelper.GetPathToWalletAsync();
			ShowFilePassword = !string.IsNullOrEmpty(WalletFilePath);
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
			if (Position == 0)
			{
				// Workaround for this bug: https://github.com/dotnet/maui/issues/9749
				Position = 1;
				Position = 0;
				Position = 1;
			}
			else
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
