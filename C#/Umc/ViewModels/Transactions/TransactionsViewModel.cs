namespace UC.Umc.ViewModels;

public partial class TransactionsViewModel : BaseViewModel
{
	private readonly ITransactionsService _service;

	[ObservableProperty]
    private CustomCollection<TransactionViewModel> _transactions = new();

	[ObservableProperty]
    private TransactionViewModel _selectedItem;

	[ObservableProperty]
    private AccountViewModel _account;

    public TransactionsViewModel(ITransactionsService service, ILogger<TransactionsViewModel> logger) : base(logger)
    {
		_service = service;
    }

	internal async Task InitializeAsync()
	{
		Transactions = await _service.GetLastAsync(SizeConstants.SizePerPageMed);
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
				Transactions = await _service.GetLastForAccountAsync(Account.Address, SizeConstants.SizePerPageMed);
			}
		}
		catch (Exception ex)
		{
            _logger.LogError(ex, "SelectAccountAsync Exception: {Ex}", ex.Message);
		}
    }

    [RelayCommand]
    private void ResetAccount()
	{
		try
		{
			Account = null;
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
