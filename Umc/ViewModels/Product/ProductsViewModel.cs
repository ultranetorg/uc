namespace UC.Umc.ViewModels;

public partial class ProductsViewModel : BasePageViewModel
{
	private readonly IProductsService _service;

	[ObservableProperty]
    private ProductViewModel _selectedItem;

	[ObservableProperty]
    private CustomCollection<ProductViewModel> _products = new();

	[ObservableProperty]
    private CustomCollection<string> _productsFilter = new();

    public ProductsViewModel(INotificationsService notificationService, IProductsService service,
		ILogger<ProductsViewModel> logger) : base(notificationService, logger)
    {
		_service = service;
	}

	[RelayCommand]
    private async Task OpenProductOptionsAsync(ProductViewModel product)
    {
		try
		{
			Guard.IsNotNull(product);

			await ShowPopup(new ProductOptionsPopup(product));
		}
		catch(ArgumentException ex)
		{
			_logger.LogError("OpenOptionsAsync: Product cannot be null, Error: {Message}", ex.Message);
		}
		catch (Exception ex)
		{
			_logger.LogError("OpenOptionsAsync Error: {Message}", ex.Message);
		}
	}

	[RelayCommand]
	private async Task RegisterProductAsync(ProductViewModel product) =>
		await Navigation.GoToAsync(nameof(ProductRegistrationPage),
			new Dictionary<string, object>() {
				{
					QueryKeys.PRODUCT, product
				}
			});

	internal async Task InitializeAsync()
	{
		var products = await _service.GetAllProductsAsync();
		Products.AddRange(products);
        ProductsFilter = DefaultDataMock.ProductsFilter;
	}
}
