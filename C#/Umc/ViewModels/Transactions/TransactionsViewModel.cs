namespace UC.Umc.ViewModels;

public partial class TransactionsViewModel : BaseViewModel
{
	private readonly ITransactionsService _service;

	[ObservableProperty]
    private TransactionViewModel _selectedItem;
    
	[ObservableProperty]
    private CustomCollection<TransactionViewModel> _transactions = new();

    public TransactionsViewModel(ITransactionsService service, ILogger<TransactionsViewModel> logger) : base(logger)
    {
		_service = service;
    }

	internal async Task InitializeAsync()
	{
		Transactions.Clear();

		var transactions = await _service.GetLastAsync(20);
		Transactions.AddRange(transactions);
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
