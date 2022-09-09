namespace UC.Net.Node.MAUI.ViewModels.Pages;

public partial class EnterPinBViewModel : BaseViewModel
{
	[ObservableProperty]
    private Wallet _wallet = DefaultDataMock.Wallet1;

    public EnterPinBViewModel(ILogger<EnterPinBViewModel> logger) : base(logger)
    {
    }

    [RelayCommand]
    private async void DeleteAsync()
    {
        await DeleteAccountPopup.Show(Wallet);
    }
	
    [RelayCommand]
    private async void TransactionsAsync()
    {
        await Shell.Current.Navigation.PushAsync(new TransactionsPage());
    }
}