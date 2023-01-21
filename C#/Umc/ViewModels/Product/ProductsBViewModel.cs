namespace UC.Umc.ViewModels;

public partial class ProductsBViewModel : BaseTransactionsViewModel
{
	private readonly IProductsService _service;

	[ObservableProperty]
    private ProductViewModel _selectedItem;

	[ObservableProperty]
    private CustomCollection<ProductViewModel> _products = new();

	[ObservableProperty]
    private CustomCollection<string> _productsFilter = new();

    public ProductsBViewModel(IProductsService service, ILogger<ProductsBViewModel> logger) : base(logger)
    {
		_service = service;
	}

	[RelayCommand]
	private async Task RegisterProductAsync(ProductViewModel product)
	{
		// await AccountOptionsPopup.Show(author);
		await Task.Delay(10);
	}

	internal async Task InitializeAsync()
	{
		var products = await _service.GetAllAsync();
		Products.AddRange(products);
        ProductsFilter = DefaultDataMock.ProductsFilter;
	}
}
