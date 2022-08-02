namespace UC.Net.Node.MAUI.ViewModels.Pages;

public partial class ProductsViewModel : BaseViewModel
{
	[ObservableProperty]
    private Product _selectedItem;
        
	[ObservableProperty]
    private CustomCollection<Product> _products = new();
    
	[ObservableProperty]
    private CustomCollection<string> _productsFilter = new();

    public ProductsViewModel(ILogger<ProductsViewModel> logger) : base(logger)
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
		// has been changes from Product to Wallet
        await AccountOptionsPopup.Show(wallet);
    }

	private void FillFakeData()
	{
        ProductsFilter = new CustomCollection<string> {"All", "To be expired", "Expired", "Hidden", "Shown" };
        Products.Add(new Product {Name = "Ultranet User Center", Owner = "ultranetorg" });
        Products.Add(new Product { Name = "Aximion3D", Owner = "ultranetorg" });
        Products.Add(new Product { Name = "Ultranet User Center", Owner = "ultranetorg" });
        Products.Add(new Product { Name = "3D UI", Owner = "ultranetorg" });
        Products.Add(new Product { Name = "Ultranet User Node", Owner = "ultranetorg" });
        Products.Add(new Product { Name = "Aximion3D", Owner = "ultranetorg" });
        Products.Add(new Product { Name = "3D UI", Owner = "ultranetorg" });
        Products.Add(new Product { Name = "Ultranet User Node", Owner = "ultranetorg" });
        Products.Add(new Product { Name = "Aximion3D", Owner = "ultranetorg" });
        Products.Add(new Product { Name = "3D UI", Owner = "ultranetorg" });
        Products.Add(new Product { Name = "Ultranet User Node", Owner = "ultranetorg" });
	}
}
