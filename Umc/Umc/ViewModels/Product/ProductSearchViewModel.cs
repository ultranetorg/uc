namespace UC.Umc.ViewModels;

public partial class ProductSearchViewModel : BasePageViewModel
{
	private readonly IProductsService _service;

	[ObservableProperty]
    private CustomCollection<ProductViewModel> _products = new();
    
	[ObservableProperty]
    private CustomCollection<FilterOption> _productsFilter = new();

	[ObservableProperty]
    private string _filter;

    public ProductSearchViewModel(INotificationsService notificationService, IProductsService service,
		ILogger<ProductSearchViewModel> logger) : base(notificationService, logger)
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

			ProductsFilter.ToList().ForEach(x => x.IsSelected = false);
			ProductsFilter.First(x => x.Name == sortBy).IsSelected = true;

			// Sort products
			var products = await _service.GetAllProductsAsync();
			var ordered = products.AsQueryable().OrderBy(x => sortBy == "Author"
				? x.Owner : sortBy == "Version"
				? x.Version : sortBy == "Name"
				? x.Name : null);
			
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
		try
		{
			InitializeLoading();

			var products = await _service.GetAllProductsAsync();
			Products.Clear();
			Products.AddRange(products);
			ProductsFilter = new CustomCollection<FilterOption>(
				DefaultDataMock.ProductsFilter.Select(f => new FilterOption { Name = f, IsSelected = false }));

			FinishLoading();
		}
		catch (Exception ex)
		{
			ToastHelper.ShowErrorMessage(_logger);
			_logger.LogError("InitializeAsync Error: {Message}", ex.Message);
		}
	}
}
