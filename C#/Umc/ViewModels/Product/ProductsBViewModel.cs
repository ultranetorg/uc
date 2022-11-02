namespace UC.Umc.ViewModels;

public partial class ProductsBViewModel : BaseTransactionsViewModel
{
	private readonly IProductsService _service;

	[ObservableProperty]
    private Product _selectedItem;

	[ObservableProperty]
    private CustomCollection<Product> _products = new();

	[ObservableProperty]
    private CustomCollection<string> _productsFilter = new();

    public ProductsBViewModel(IProductsService service, ILogger<ProductsBViewModel> logger) : base(logger)
    {
		_service = service;
    }

	internal async Task InitializeAsync()
	{
		var products = await _service.GetAllAsync();
		Products.AddRange(products);
        ProductsFilter = DefaultDataMock.DefaultFilter;
	}
}
