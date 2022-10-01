namespace UC.Net.Node.MAUI.ViewModels.Pages;

public partial class PrivateKeyViewModel : BaseViewModel
{
	[ObservableProperty]
    private CustomCollection<Author> _authors = new();

	[ObservableProperty]
    private CustomCollection<Product> _products = new();
	
	[ObservableProperty]
    private Wallet _wallet;

    public PrivateKeyViewModel(ILogger<PrivateKeyViewModel> logger) : base(logger)
    { 
    }

	[RelayCommand]
    private async Task DeleteAsync()
    {
        await DeleteAccountPopup.Show(Wallet);
    }
}
