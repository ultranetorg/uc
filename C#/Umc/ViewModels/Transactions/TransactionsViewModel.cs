namespace UC.Umc.ViewModels;

public partial class TransactionsViewModel : BaseViewModel
{
	private readonly ITransactionsService _service;

	[ObservableProperty]
    private CustomCollection<TransactionViewModel> _transactions = new();

	[ObservableProperty]
    private string _filter;

	[ObservableProperty]
    private AccountViewModel _account;

    public TransactionsViewModel(ITransactionsService service, ILogger<TransactionsViewModel> logger) : base(logger)
    {
		_service = service;
    }

	internal async Task InitializeAsync()
	{
		Transactions = await _service.ListTransactionsAsync(null, null, SizeConstants.SizePerPageMed);
	}
	
	[RelayCommand]
    public async Task SearchTransactionsAsync()
    {
		try
		{
			Guard.IsNotNull(Filter);

			InitializeLoading();

			// Search transactions
			var transactions = await _service.ListTransactionsAsync(Account?.Address, Filter, SizeConstants.SizePerPageMed);

			Transactions.Clear();
			Transactions.AddRange(transactions);
			
			FinishLoading();
		}
		catch (Exception ex)
		{
			ToastHelper.ShowErrorMessage(_logger);
			_logger.LogError("SearchTransactionsAsync Error: {Message}", ex.Message);
		}
    }

    [RelayCommand]
    private async Task SelectAccountAsync()
	{
		try
		{
			var popup = new SourceAccountPopup();
			await ShowPopup(popup);
			if (popup.Vm?.Account != null)
			{
				Account = popup.Vm.Account;
				Transactions = await _service.ListTransactionsAsync(Account.Address, Filter, SizeConstants.SizePerPageMed);
			}
		}
		catch (Exception ex)
		{
            _logger.LogError(ex, "SelectAccountAsync Exception: {Ex}", ex.Message);
		}
    }

    [RelayCommand]
    private async Task ResetAccountAsync()
	{
		try
		{
			Account = null;
			await InitializeAsync();
		}
		catch (Exception ex)
		{
            _logger.LogError(ex, "ResetAccount Exception: {Ex}", ex.Message);
		}
    }

	[RelayCommand]
    private async Task OpenDetailsAsync(TransactionViewModel transaction)
	{
		try
		{
			Guard.IsNotNull(transaction);

			await ShowPopup(new TransactionPopup(transaction));
		}
		catch(ArgumentException ex)
		{
			_logger.LogError("OpenDetailsAsync: Transaction cannot be null, Error: {Message}", ex.Message);
		}
		catch (Exception ex)
		{
			_logger.LogError("OpenDetailsAsync Error: {Message}", ex.Message);
		}
	}
}
