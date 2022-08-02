namespace UC.Net.Node.MAUI.ViewModels.Pages;

public partial class ProductsBViewModel : BaseViewModel
{
	[ObservableProperty]
    private Product _selectedItem;

	[ObservableProperty]
    private CustomCollection<Product> _products = new();

	[ObservableProperty]
    private CustomCollection<string> _productsFilter = new();

    public ProductsBViewModel(ILogger<ProductsBViewModel> logger) : base(logger)
    {
		FillFakeData();
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
    private void ItemTappedAsync(Product Product)
    {
    }
	
	[RelayCommand]
    private async void OptionsAsync(Wallet wallet)
    {
		// has been changed from Product to Wallet
		await AccountOptionsPopup.Show(wallet);
	}

	private void FillFakeData()
	{
        ProductsFilter = new CustomCollection<string> {"All", "To be expired", "Expired", "Hidden", "Shown" };
        Products.Add(new Product {Name = "Ultranet User Center", Color = Color.FromArgb("#4900E3"), Initl ="U", Owner = "ultranetorg" });
        Products.Add(new Product { Name = "Aximion3D",Color=Color.FromArgb("#18C6A6"), Initl = "A", Owner = "ultranetorg" });
        Products.Add(new Product { Name = "3D UI", Color = Color.FromArgb("#EE7636"), Initl = "3", Owner = "ultranetorg" });
        Products.Add(new Product { Name = "Ultranet User Node", Color = Color.FromArgb("#4900E3"), Initl = "U", Owner = "ultranetorg" });
        Products.Add(new Product { Name = "Aximion3D", Color = Color.FromArgb("#18C6A6"), Initl = "A", Owner = "ultranetorg" });
        Products.Add(new Product { Name = "3D UI", Color = Color.FromArgb("#EE7636"), Initl = "3", Owner = "ultranetorg" });
        Products.Add(new Product { Name = "Ultranet User Node", Color = Color.FromArgb("#4900E3"), Initl = "U", Owner = "ultranetorg" });
        Products.Add(new Product { Name = "Aximion3D", Color = Color.FromArgb("#18C6A6"), Initl = "A", Owner = "ultranetorg" });
        Products.Add(new Product { Name = "3D UI", Color = Color.FromArgb("#EE7636"), Initl = "3", Owner = "ultranetorg" });
        Products.Add(new Product { Name = "Ultranet User Node", Color = Color.FromArgb("#4900E3"), Initl = "U", Owner = "ultranetorg" });
	}
}
