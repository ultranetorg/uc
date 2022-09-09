namespace UC.Net.Node.MAUI.ViewModels.Pages;

public partial class ProductSearchViewModel : BaseTransactionsViewModel
{
	private readonly IProductsService _service;
	[ObservableProperty]
    private CustomCollection<Product> _products = new();
    
	[ObservableProperty]
    private CustomCollection<string> _productsFilter = new();

    public ProductSearchViewModel(IProductsService service, ILogger<ProductSearchViewModel> logger) : base(logger)
    {
		_service = service;
    }

	internal async Task InitializeAsync()
	{
		var products = await _service.GetAllAsync();
		Products.AddRange(products);
        ProductsFilter = DefaultDataMock.ProductsFilter2;
	}
}
