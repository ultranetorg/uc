namespace UC.Umc.ViewModels;

public partial class ProductsListViewModel : BaseViewModel
{
	private readonly IProductsService _service;

	[ObservableProperty]
    private ProductViewModel _selectedItem;
        
	[ObservableProperty]
    private CustomCollection<ProductViewModel> _products = new();
    
	[ObservableProperty]
    private CustomCollection<string> _productsFilter = new();

    public ProductsListViewModel(IProductsService service, ILogger<ProductsListViewModel> logger) : base(logger)
    {
		_service = service;
    }

	[RelayCommand]
    private async Task OpenProductOptionsAsync(ProductViewModel product)
    {
        // await AccountOptionsPopup.Show(author);
		await Task.Delay(10);
	}

	[RelayCommand]
	private async Task RegisterProductAsync(ProductViewModel product)
	{
		// await AccountOptionsPopup.Show(author);
		await Task.Delay(10);
	}

	internal async Task InitializeAsync()
	{
		var products = await _service.GetAllProductsAsync();
		Products.AddRange(products);
        ProductsFilter = DefaultDataMock.ProductsFilter;
	}
}
