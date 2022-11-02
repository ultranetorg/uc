namespace UC.Umc.ViewModels;

public partial class TransactionsViewModel : BaseTransactionsViewModel
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
}
