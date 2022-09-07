namespace UC.Net.Node.MAUI.ViewModels.Pages;

public partial class TransactionsBViewModel : BaseTransactionsViewModel
{
	private readonly ITransactionsService _service;

    private TransactionList TransactionsThisWeek;
    private TransactionList TransactionsLastWeek;

	[ObservableProperty]
    private CustomCollection<TransactionList> _transactions = new();

	[ObservableProperty]
    private Wallet _wallet;

    public TransactionsBViewModel(ITransactionsService service, ILogger<TransactionsBViewModel> logger) : base(logger)
    {
		_service = service;
    }

	internal async Task InitializeAsync()
	{
		var transactions = await _service.GetLastAsync(20);
		TransactionsThisWeek.AddRange(transactions.Take(10));
		TransactionsLastWeek.AddRange(transactions.Skip(10));

		Transactions.Add(TransactionsThisWeek);
		Transactions.Add(TransactionsLastWeek);
	}
}
