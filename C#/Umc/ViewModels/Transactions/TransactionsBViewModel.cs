namespace UC.Umc.ViewModels;

public partial class TransactionsBViewModel : BaseViewModel
{
	private readonly ITransactionsService _service;

    private readonly TransactionList TransactionsThisWeek = new("This Week");
    private readonly TransactionList TransactionsLastWeek = new("Last Week");

	[ObservableProperty]
    private CustomCollection<TransactionList> _transactions = new();

	[ObservableProperty]
    private AccountViewModel _account;

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
