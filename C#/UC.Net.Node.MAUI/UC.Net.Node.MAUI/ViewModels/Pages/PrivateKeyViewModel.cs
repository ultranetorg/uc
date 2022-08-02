namespace UC.Net.Node.MAUI.ViewModels.Pages;

public partial class PrivateKeyViewModel : BaseViewModel
{
	[ObservableProperty]
    private CustomCollection<Author> _authors = new();

	[ObservableProperty]
    private CustomCollection<Product> _products = new();
	
	[ObservableProperty]
    private Wallet _wallet;

    public PrivateKeyViewModel(Wallet wallet, ILogger<PrivateKeyViewModel> logger) : base(logger)
    {
        Wallet = wallet;   
    }

	[RelayCommand]
    private async void DeleteAsync()
    {
        await DeleteAccountPopup.Show(Wallet);
    }
}
