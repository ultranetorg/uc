namespace UC.Umc.ViewModels;

public partial class ETHTransfer1ViewModel : BaseViewModel
{
	[ObservableProperty]
    private AccountViewModel _account;

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
	[NotifyPropertyChangedFor(nameof(UsdAmount))]
	private decimal _ethAmount;

	public decimal UsdAmount => EthAmount;

	public ETHTransfer1ViewModel(ILogger<ETHTransfer1ViewModel> logger): base(logger)
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
}
