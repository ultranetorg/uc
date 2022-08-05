namespace UC.Net.Node.MAUI.ViewModels.Pages;

public partial class ProductsBViewModel : BaseTransactionsViewModel
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

	private void FillFakeData()
	{
        ProductsFilter = new CustomCollection<string> {"All", "To be expired", "Expired", "Hidden", "Shown" };
        Products.Add(new Product {Name = "Ultranet User Center", Color = Color.FromArgb("#4900E3"), Initl = "U", Owner = "ultranetorg" });
        Products.Add(new Product { Name = "Aximion3D",Color = Color.FromArgb("#18C6A6"), Initl = "A", Owner = "ultranetorg" });
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
