namespace UC.Net.Node.MAUI.ViewModels;

public abstract partial class BaseTransactionsViewModel : BaseViewModel
{
	protected BaseTransactionsViewModel(ILogger logger) : base(logger)
	{
	}

	[RelayCommand]
    private async Task ItemTappedAsync(TransactionViewModel transaction)
    {
        if (transaction == null) return;
        if (transaction.Status == TransactionStatus.Pending)
            await Shell.Current.Navigation.PushAsync(new UnfinishTransferPage());
        else
            await TransactionPopup.Show(transaction);
    }

	[RelayCommand]
    private void ItemTappedEmptyAsync(Product Product)
    {
		// TBD
    }

	[RelayCommand]
    private async Task OptionsAsync(TransactionViewModel Transaction)
    {
        if (Transaction.Status == TransactionStatus.Pending)
            await Shell.Current.Navigation.PushAsync(new UnfinishTransferPage());
        else
            await TransactionPopup.Show(Transaction);
    }
}
