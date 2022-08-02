namespace UC.Net.Node.MAUI.ViewModels.Pages;

public partial class EnterPinBViewModel : BaseViewModel
{
	[ObservableProperty]
    private Wallet _wallet = new()
    {
        Id = Guid.NewGuid(),
        Unts = 5005,
        IconCode = "47F0",
        Name = "Main ultranet wallet",
        AccountColor = Color.FromArgb("#6601e3"),
    };

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