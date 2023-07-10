namespace UC.Umc.ViewModels.Popups;

public partial class TransactionDetailsViewModel : BaseViewModel
{
	[ObservableProperty]
    private TransactionViewModel _transaction;

	[ObservableProperty]
    private AccountViewModel _account;

	public TransactionDetailsViewModel(ILogger<TransactionDetailsViewModel> logger) : base(logger)
	{
	}

	[RelayCommand]
	private async Task CopyHashAsync()
	{
        try
        {
			await Clipboard.SetTextAsync(Transaction.Hash);
            await ToastHelper.ShowMessageAsync(Properties.Additional_Strings.Message_Copied);
#if DEBUG
            _logger.LogDebug("CopyHashAsync Address: {Address}", Transaction.Hash);
#endif
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CopyHashAsync Exception: {Ex}", ex.Message);
            ToastHelper.ShowErrorMessage(_logger);
        }
	}
}
