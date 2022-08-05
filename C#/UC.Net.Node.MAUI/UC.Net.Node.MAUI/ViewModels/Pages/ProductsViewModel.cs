namespace UC.Net.Node.MAUI.ViewModels.Pages;

public partial class ProductsViewModel : BaseTransactionsViewModel
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
