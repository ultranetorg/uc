namespace UC.Net.Node.MAUI.ViewModels.Pages;

public partial class DeleteAccountViewModel : BaseViewModel
{
	[ObservableProperty]
    private CustomCollection<Author> _authors = new();

	[ObservableProperty]
    private CustomCollection<Product> _products = new();

	[ObservableProperty]
    private Wallet _wallet;

    public DeleteAccountViewModel(ILogger<DeleteAccountViewModel> logger) : base(logger)
    {
    }

    [RelayCommand]
    private async void DeleteAsync()
    {
        await DeleteAccountPopup.Show(Wallet);
    }

	public void FillFakeData(Wallet wallet)
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