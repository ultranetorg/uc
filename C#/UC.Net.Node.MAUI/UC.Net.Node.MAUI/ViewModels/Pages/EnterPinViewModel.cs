namespace UC.Net.Node.MAUI.ViewModels.Pages;

public partial class EnterPinViewModel : BaseViewModel
{
	[ObservableProperty]
    private Account _account = DefaultDataMock.Account1;

    public EnterPinViewModel(ILogger<EnterPinViewModel> logger) : base(logger)
    {
    }

	[RelayCommand]
    private async Task DeleteAsync()
    {
        await DeleteAccountPopup.Show(Account);
    }

	[RelayCommand]
    private async Task TransactionsAsync()
    {
        await Shell.Current.Navigation.PushAsync(new TransactionsPage());
    }
}
