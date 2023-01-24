namespace UC.Umc.ViewModels;

public partial class ProductSearchViewModel : BaseTransactionsViewModel
{
	private readonly IProductsService _service;
	[ObservableProperty]
    private CustomCollection<ProductViewModel> _products = new();
    
	[ObservableProperty]
    private CustomCollection<string> _productsFilter = new();

	[ObservableProperty]
    private string _filter;

    public ProductSearchViewModel(IProductsService service, ILogger<ProductSearchViewModel> logger) : base(logger)
    {
		_service = service;
    }
	
	[RelayCommand]
    public async Task SearchProductsAsync()
    {
		try
		{
			Guard.IsNotNull(Filter);

			InitializeLoading();

			// Search products
			var products = await _service.SearchProductsAsync(Filter);

			Products.Clear();
			Products.AddRange(products);
			
			FinishLoading();
		}
		catch (Exception ex)
		{
			ToastHelper.ShowErrorMessage(_logger);
			_logger.LogError("SearchProductsAsync Error: {Message}", ex.Message);
		}
    }

	[RelayCommand]
	private async Task SortProductsAsync()
    {
		// Products.OrderBy(x => x.Name);
		await Task.Delay(10);
    }

	internal async Task InitializeAsync()
	{
		var products = await _service.GetAllProductsAsync();
		Products.AddRange(products);
        ProductsFilter = DefaultDataMock.ProductsFilter;
	}
}
