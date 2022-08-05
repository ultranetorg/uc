namespace UC.Net.Node.MAUI.ViewModels.Pages;

public partial class ProductSearchViewModel : BaseTransactionsViewModel
{
	[ObservableProperty]
    private CustomCollection<Product> _products = new();
    
	[ObservableProperty]
    private CustomCollection<string> _productsFilter = new();

    public ProductSearchViewModel(ILogger<ProductSearchViewModel> logger) : base(logger)
    {
		FillFakeData();
    }

	private void FillFakeData()
	{
        ProductsFilter = new CustomCollection<string> { "Recent", "By author" };
        Products.Add(new Product { Name = "Edge Browser", Owner = "microsoft" });
        Products.Add(new Product { Name = "Amazon Helmet", Owner = "Amazon" });
        Products.Add(new Product { Name = "Chrome", Owner = "Google" });
        Products.Add(new Product { Name = "Edge Browser", Owner = "microsoft" });
        Products.Add(new Product { Name = "Amazon Helmet", Owner = "Amazon" });
        Products.Add(new Product { Name = "Chrome", Owner = "Google" });
        Products.Add(new Product { Name = "Edge Browser", Owner = "microsoft" });
        Products.Add(new Product { Name = "Amazon Helmet", Owner = "Amazon" });
        Products.Add(new Product { Name = "Chrome", Owner = "Google" });
        Products.Add(new Product { Name = "Edge Browser", Owner = "microsoft" });
        Products.Add(new Product { Name = "Amazon Helmet", Owner = "Amazon" });
        Products.Add(new Product { Name = "Chrome", Owner = "Google" });
        Products.Add(new Product { Name = "Edge Browser", Owner = "microsoft" });
        Products.Add(new Product { Name = "Amazon Helmet", Owner = "Amazon" });
        Products.Add(new Product { Name = "Chrome", Owner = "Google" });
        Products.Add(new Product { Name = "Edge Browser", Owner = "microsoft" });
        Products.Add(new Product { Name = "Amazon Helmet", Owner = "Amazon" });
        Products.Add(new Product { Name = "Chrome", Owner = "Google" });
        Products.Add(new Product { Name = "Edge Browser", Owner = "microsoft" });
        Products.Add(new Product { Name = "Amazon Helmet", Owner = "Amazon" });
        Products.Add(new Product { Name = "Chrome", Owner = "Google" });
        Products.Add(new Product { Name = "Edge Browser", Owner = "microsoft" });
        Products.Add(new Product { Name = "Amazon Helmet", Owner = "Amazon" });
        Products.Add(new Product { Name = "Chrome", Owner = "Google" });
        Products.Add(new Product { Name = "Edge Browser", Owner = "microsoft" });
        Products.Add(new Product { Name = "Amazon Helmet", Owner = "Amazon" });
        Products.Add(new Product { Name = "Chrome", Owner = "Google" });
        Products.Add(new Product { Name = "Edge Browser", Owner = "microsoft" });
        Products.Add(new Product { Name = "Amazon Helmet", Owner = "Amazon" });
        Products.Add(new Product { Name = "Chrome", Owner = "Google" });
        Products.Add(new Product { Name = "Edge Browser", Owner = "microsoft" });
        Products.Add(new Product { Name = "Amazon Helmet", Owner = "Amazon" });
        Products.Add(new Product { Name = "Chrome", Owner = "Google" });
        Products.Add(new Product { Name = "Edge Browser", Owner = "microsoft" });
        Products.Add(new Product { Name = "Amazon Helmet", Owner = "Amazon" });
        Products.Add(new Product { Name = "Chrome", Owner = "Google" });
	}
}
