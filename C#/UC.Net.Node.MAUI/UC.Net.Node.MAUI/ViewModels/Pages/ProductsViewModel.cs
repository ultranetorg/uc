namespace UC.Net.Node.MAUI.ViewModels.Pages;

public partial class ProductsViewModel : BaseTransactionsViewModel
{
	private readonly IProductsService _service;

	[ObservableProperty]
    private Product _selectedItem;
        
	[ObservableProperty]
    private CustomCollection<Product> _products = new();
    
	[ObservableProperty]
    private CustomCollection<string> _productsFilter = new();

    public ProductsViewModel(IProductsService service, ILogger<ProductsViewModel> logger) : base(logger)
    {
		_service = service;
    }

	internal async Task InitializeAsync()
	{
		var products = await _service.GetAllAsync();
		Products.AddRange(products);
        ProductsFilter = new CustomCollection<string> {"All", "To be expired", "Expired", "Hidden", "Shown" };
	}
}
