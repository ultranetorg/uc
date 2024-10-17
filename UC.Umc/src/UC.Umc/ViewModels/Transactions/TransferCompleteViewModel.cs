using UC.Umc.Common.Constants;
using UC.Umc.Common.Helpers;
using UC.Umc.Models;

namespace UC.Umc.ViewModels.Transactions;

public partial class TransferCompleteViewModel : BaseViewModel
{
	[ObservableProperty]
	private AccountModel _account;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(UntComission))]
	[NotifyPropertyChangedFor(nameof(EthComission))]
	private decimal _untAmount;

	public decimal UntComission => (UntAmount + 1) / 10;
	public decimal EthComission => (UntAmount + 1) / 100;
	public string TransactionDate => "10/15/2021 19:24";

	public TransferCompleteViewModel(ILogger<TransferCompleteViewModel> logger) : base(logger)
	{
	}

	public override void ApplyQueryAttributes(IDictionary<string, object> query)
	{
		try
		{
			InitializeLoading();
			
			Account = (AccountModel)query[QueryKeys.ACCOUNT];
			UntAmount = (decimal)query[QueryKeys.UNT_AMOUNT];
#if DEBUG
			_logger.LogDebug("ApplyQueryAttributes Account: {Account}", Account);
			_logger.LogDebug("ApplyQueryAttributes UntAmount: {UntAmount}", UntAmount);
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
	private async Task TransactionsAsync()
	{
		try
		{
			await Navigation.GoToUpwardsAsync(Routes.TRANSACTIONS);
		}
		catch (Exception ex)
		{
			_logger.LogError("TransactionsAsync Error: {Message}", ex.Message);
		}
	}
}