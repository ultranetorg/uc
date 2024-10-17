using UC.Umc.Common.Helpers;
using UC.Umc.Models;

namespace UC.Umc.ViewModels.Popups;

public partial class TransactionDetailsViewModel(ILogger<TransactionDetailsViewModel> logger) : BaseViewModel(logger)
{
	[ObservableProperty]
	private TransactionModel _transaction;

	[ObservableProperty]
	private AccountModel _account;

	[RelayCommand]
	private async Task CopyHashAsync()
	{
		try
		{
			await Clipboard.SetTextAsync(Transaction.Hash);
			await ToastHelper.ShowMessageAsync("Copied to clipboard");
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
