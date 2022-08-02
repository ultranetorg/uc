namespace UC.Net.Node.MAUI.ViewModels.Pages;

public partial class DeleteAccountViewModel : BaseViewModel
{
	[ObservableProperty]
    private CustomCollection<Author> _authors = new CustomCollection<Author>();

	[ObservableProperty]
    private CustomCollection<Product> _products = new CustomCollection<Product>();

	[ObservableProperty]
    private Wallet _wallet;

    public DeleteAccountViewModel(Wallet wallet, ILogger<DeleteAccountViewModel> logger) : base(logger)
    {
		FillFakeData(wallet);
    }

    [RelayCommand]
    private async void DeleteAsync()
    {
        await DeleteAccountPopup.Show(Wallet);
    }

	private void FillFakeData(Wallet wallet)
	{
		Wallet = wallet;          
        Authors.Add(new Author { Name = "ultranet" });
        Authors.Add(new Author { Name = "ultranetorganization" });
        Authors.Add(new Author { Name = "aximion" });
        Products.Add(new Product { Name = "UNS" });
        Products.Add(new Product { Name = "Aximion3D" });
        Products.Add(new Product { Name = "ultranet" });
	}
}