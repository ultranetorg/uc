namespace UC.Umc.ViewModels;

public partial class ETHTransferViewModel : BaseAccountViewModel
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
	[NotifyPropertyChangedFor(nameof(UntAmount))]
	[NotifyPropertyChangedFor(nameof(EthCommission))]
	[NotifyPropertyChangedFor(nameof(UntCommission))]
	private decimal _ethAmount;

	public decimal UntAmount => EthAmount * 10;
	public decimal EthCommission => (EthAmount + 1) / 100;
	public decimal UntCommission => (EthAmount + 1) / 10;

    public ETHTransferViewModel(ILogger<ETHTransferViewModel> logger) : base(logger)
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
	private async Task ShowAccountsPopupAsync()
	{
		try
		{
			var popup = new SourceAccountPopup();
			await ShowPopup(popup);

			if (popup?.Vm?.Account != null)
			{
				Account = popup.Vm.Account;
			}
		}
		catch (Exception ex)
		{
			_logger.LogError("ShowAccountsPopupAsync Error: {Message}", ex.Message);
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
			else if (Position == 1)
			{
				Position = 2;
			}
			else
			{
				await Navigation.PopAsync();
				await ToastHelper.ShowMessageAsync("Successfully transfered!");
			}

			var account = Account;
		}
		catch (Exception ex)
		{
			_logger.LogError("NextWorkaroundAsync Error: {Message}", ex.Message);
		}
		
	}

	[RelayCommand]
    private async Task OpenOptionsPopupAsync()
    {
		try
		{
			await ShowPopup(new TransferOptionsPopup());
		}
		catch (Exception ex)
		{
			_logger.LogError("OpenOptionsPopupAsync Error: {Message}", ex.Message);
		}
    }

	[RelayCommand]
    private async Task ConfirmAsync()
    {
		try
		{
			await Navigation.GoToAsync(nameof(TransferCompletePage));
		}
		catch (Exception ex)
		{
			_logger.LogError("ConfirmAsync Error: {Message}", ex.Message);
		}
    }
}
