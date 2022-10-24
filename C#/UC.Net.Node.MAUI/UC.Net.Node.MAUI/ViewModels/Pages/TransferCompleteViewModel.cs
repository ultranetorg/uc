namespace UC.Net.Node.MAUI.ViewModels.Pages;

public partial class TransferCompleteViewModel : BaseViewModel
{
	[ObservableProperty]
    private Account _account = DefaultDataMock.Account1;

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
        await DeleteAccountPopup.Show(Account);
    }
}