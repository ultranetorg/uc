namespace UC.Umc.ViewModels;

public partial class TransferCompleteViewModel : BasePageViewModel
{
	[ObservableProperty]
    private AccountViewModel _account;
	[ObservableProperty]
    private string _sourceAccount;

	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(UntComission))]
	[NotifyPropertyChangedFor(nameof(EthComission))]
	private decimal _untAmount;

	public decimal UntComission => (UntAmount + 1) / 10;
	public decimal EthComission => (UntAmount + 1) / 100;
	public string TransactionDate => "10/15/2021 19:24";

    public TransferCompleteViewModel(INotificationsService notificationService,
		ILogger<TransferCompleteViewModel> logger) : base(notificationService, logger)
    {
    }

    public override void ApplyQueryAttributes(IDictionary<string, object> query)
	{
        try
        {
            InitializeLoading();
			
            Account = (AccountViewModel)query[QueryKeys.ACCOUNT];
            UntAmount = (decimal)query[QueryKeys.UNT_AMOUNT];
            SourceAccount = (string)query[QueryKeys.SOURCE_ACCOUNT];
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
			await Navigation.GoToTransactionsAsync();
		}
		catch (Exception ex)
		{
			_logger.LogError("TransactionsAsync Error: {Message}", ex.Message);
		}
    }
}