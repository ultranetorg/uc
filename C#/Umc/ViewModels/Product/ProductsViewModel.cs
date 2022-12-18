namespace UC.Umc.ViewModels;

public partial class ProductsViewModel : BaseTransactionsViewModel
{
	private readonly IProductsService _service;

	[ObservableProperty]
    private ProductViewModel _selectedItem;
        
	[ObservableProperty]
    private CustomCollection<ProductViewModel> _products = new();
    
	[ObservableProperty]
    private CustomCollection<string> _productsFilter = new();

    public ProductsViewModel(IProductsService service, ILogger<ProductsViewModel> logger) : base(logger)
    {
		_service = service;
    }

	[RelayCommand]
    private async Task OpenProductOptionsAsync(ProductViewModel product)
    {
        // await AccountOptionsPopup.Show(author);
		await Task.Delay(10);
    }

	internal async Task InitializeAsync()
	{
		var products = await _service.GetAllAsync();
		Products.AddRange(products);
        ProductsFilter = DefaultDataMock.DefaultFilter;
	}
}
