namespace UC.Umc.ViewModels;

public partial class ETHTransferViewModel : BaseAccountViewModel
{
	[ObservableProperty]
	private bool _needToFinishTransfer = true;

	[ObservableProperty]
	private bool _isPrivateKey = true;

	[ObservableProperty]
	private bool _isFilePath;

	[ObservableProperty]
	private bool _showPassword;

	[ObservableProperty]
	private string _privateKey;

	[ObservableProperty]
	private string _walletPath;

	[ObservableProperty]
	private string _walletPassword;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(UntAmount))]
	[NotifyPropertyChangedFor(nameof(EthCommission))]
	[NotifyPropertyChangedFor(nameof(UntCommission))]
	private decimal _ethAmount = 10;

	public decimal UntAmount => EthAmount * 10;
	public decimal EthCommission => (EthAmount + 1) / 100;
	public decimal UntCommission => (EthAmount + 1) / 10;

    public ETHTransferViewModel(INotificationsService notificationService, ILogger<ETHTransferViewModel> logger) : base(notificationService, logger)
    {
    }

    public override void ApplyQueryAttributes(IDictionary<string, object> query)
	{
        try
        {
            InitializeLoading();
			ClearFields();

			Account = (AccountViewModel)query[QueryKeys.ACCOUNT];
			EthAmount = (decimal)query[QueryKeys.ETH_AMOUNT];
#if DEBUG
			_logger.LogDebug("ApplyQueryAttributes Account: {Account}", Account);
			_logger.LogDebug("ApplyQueryAttributes EthAmount: {EthAmount}", Account);
#endif
		}
		catch (Exception ex)
        {
            _logger.LogError(ex, "ApplyQueryAttributes Exception: {Ex}", ex.Message);
            ToastHelper.ShowErrorMessage(_logger);
        }
        finally
        {
            FinishLoading();
        }
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
			ShowPassword = !string.IsNullOrEmpty(WalletPath);
		}
		catch (Exception ex)
		{
			_logger.LogError("OpenFilePickerAsync Error: {Message}", ex.Message);
		}
	}
	
	[RelayCommand]
	private async Task OpenUnfinishedTransferPageAsync()
	{
		try
		{
			await Navigation.GoToAsync(nameof(UnfinishTransferPage));
		}
		catch (Exception ex)
		{
			_logger.LogError("TransferProductAsync Error: {Message}", ex.Message);
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
	private void NextWorkaround()
	{
		try
		{
			var isValidStep = EthAmount > 0 && (IsPrivateKey && !string.IsNullOrEmpty(PrivateKey))
				|| (IsFilePath && !string.IsNullOrEmpty(WalletPath) && !string.IsNullOrEmpty(WalletPassword));

			if (Position == 0 && isValidStep)
			{
				// Workaround for this bug: https://github.com/dotnet/maui/issues/9749
				Position = 1;
				Position = 0;
				Position = 1;
			}
			else if (Position == 1 && Account != null)
			{
				Position = 2;
			}
		}
		catch (Exception ex)
		{
			_logger.LogError("NextWorkaroundAsync Error: {Message}", ex.Message);
		}
	}

	[RelayCommand]
    private async Task ConfirmAsync()
    {
		try
		{
			await Navigation.GoToAsync(Routes.COMPLETED_TRANSFERS, new Dictionary<string, object>()
			{
				{QueryKeys.ACCOUNT, Account},
				{QueryKeys.UNT_AMOUNT, UntAmount },
				{QueryKeys.SOURCE_ACCOUNT, PrivateKey ?? WalletPath }
			});

			ClearFields();
		}
		catch (Exception ex)
		{
			_logger.LogError("ConfirmAsync Error: {Message}", ex.Message);
		}
    }

	private void ClearFields()
	{
		Position = 0;
		Account = null;
		EthAmount = 10;
		IsPrivateKey = true;
		IsFilePath = false;
		PrivateKey = string.Empty;
		WalletPath = string.Empty;
		WalletPassword = string.Empty;
	}
}
