namespace UC.Net.Node.MAUI.ViewModels.Pages;

public partial class TransferCompleteViewModel : BaseViewModel
{
	[ObservableProperty]
    private Wallet _wallet = DefaultDataMock.Wallet1;

    public TransferCompleteViewModel(ILogger<TransferCompleteViewModel> logger) : base(logger)
    {
    }
	
	[RelayCommand]
    private async Task TransactionsAsync()
    {
        await Shell.Current.Navigation.PushAsync(new TransactionsPage());
    }

	[RelayCommand]
    private async Task DeleteAsync()
    {
        await DeleteAccountPopup.Show(Wallet);
    }
}