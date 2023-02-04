namespace UC.Umc.ViewModels.Popups;

public partial class TransactionDetailsViewModel : BaseViewModel
{
	[ObservableProperty]
    public TransactionViewModel _transaction;
	[ObservableProperty]
    public AccountViewModel _account;

	public TransactionDetailsViewModel(ILogger<TransactionDetailsViewModel> logger) : base(logger)
	{
	}
}
