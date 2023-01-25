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
	private async Task SortProductsAsync(string sortBy)
    {
		try
		{
			Guard.IsNotNullOrEmpty(sortBy);

			InitializeLoading();

			// Sort products
			var products = await _service.GetAllProductsAsync();
			var ordered = products.AsQueryable().OrderBy(x => sortBy == "By Authors"
				? x.Author.Name : sortBy == "By Version"
				? x.Version : x.Name);
			
			Products.Clear();
			Products.AddRange(ordered);
			
			FinishLoading();
		}
		catch (Exception ex)
		{
			ToastHelper.ShowErrorMessage(_logger);
			_logger.LogError("SortProductsAsync Error: {Message}", ex.Message);
		}
    }

	internal async Task InitializeAsync()
	{
		var products = await _service.GetAllProductsAsync();
		Products.AddRange(products);
        ProductsFilter = DefaultDataMock.ProductsFilter;
	}
}
