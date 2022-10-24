namespace UC.Net.Node.MAUI.ViewModels;

public partial class BaseTransactionsViewModel : BaseViewModel
{
	protected BaseTransactionsViewModel(ILogger logger) : base(logger)
	{
	}

	[RelayCommand]
    private async Task CreateAsync()
    {
        await Shell.Current.Navigation.PushModalAsync(new CreateAccountPage());
    }

	[RelayCommand]
    private async Task RestoreAsync()
    {
        await Shell.Current.Navigation.PushAsync(new RestoreAccountPage());
    }

	[RelayCommand]
    private async Task ItemTappedAsync(Transaction Transaction)
    {
        if (Transaction == null) return;
        if (Transaction.Status == TransactionStatus.Pending)
            await Shell.Current.Navigation.PushAsync(new UnfinishTransferPage());
        else
            await TransactionPopup.Show(Transaction);
    }

	[RelayCommand]
    private void ItemTappedEmptyAsync(Product Product)
    {
		// TBD
    }

	[RelayCommand]
    private async Task OptionsAsync(Transaction Transaction)
    {
        if (Transaction.Status == TransactionStatus.Pending)
            await Shell.Current.Navigation.PushAsync(new UnfinishTransferPage());
        else
            await TransactionPopup.Show(Transaction);
    }

	[RelayCommand]
    private async Task OpenOptionsAsync(Account account)
    {
        await AccountOptionsPopup.Show(account);
    }
}
