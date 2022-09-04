namespace UC.Net.Node.MAUI.ViewModels;

public partial class BaseTransactionsViewModel : BaseViewModel
{
	protected BaseTransactionsViewModel(ILogger logger) : base(logger)
	{
	}

	[RelayCommand]
    private async void CreateAsync()
    {
        await Shell.Current.Navigation.PushModalAsync(new CreateAccountPage());
    }

	[RelayCommand]
    private async void RestoreAsync()
    {
        await Shell.Current.Navigation.PushAsync(new RestoreAccountPage());
    }

	[RelayCommand]
    private async void ItemTappedAsync(Transaction Transaction)
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
    private async void OptionsAsync(Transaction Transaction)
    {
        if (Transaction.Status == TransactionStatus.Pending)
            await Shell.Current.Navigation.PushAsync(new UnfinishTransferPage());
        else
            await TransactionPopup.Show(Transaction);
    }

	[RelayCommand]
    private async void OpenOptionsAsync(Wallet wallet)
    {
        await AccountOptionsPopup.Show(wallet);
    }
}
