namespace UC.Umc.ViewModels;

public partial class AuthorDetailsViewModel : BaseAuthorViewModel
{
	private readonly IServicesMockData _service;
	private readonly IProductsService _productsService;
	
	[ObservableProperty]
    private CustomCollection<ProductViewModel> _products = new();
	
	[ObservableProperty]
    private CustomCollection<Bid> _bidsHistory = new();

    public AuthorDetailsViewModel(INotificationsService notificationService, IProductsService productsService, IServicesMockData service,
		ILogger<AuthorDetailsViewModel> logger) : base(notificationService, logger)
    {
		_service = service;
		_productsService = productsService;
    }

    public override void ApplyQueryAttributes(IDictionary<string, object> query)
	{
        try
        {
            InitializeLoading();

            Author = (AuthorViewModel)query[QueryKeys.AUTHOR];
#if DEBUG
            _logger.LogDebug("ApplyQueryAttributes Author: {Author}", Author);
#endif
			Products = _productsService.GetAccountProducts(Author.Account.Address);

			BidsHistory = new(_service.BidsHistory);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ApplyQueryAttributes Exception: {Ex}", ex.Message);
            ToastHelper.ShowErrorMessage(_logger);
        }
        finally
        {
            FinishLoading();
        }
	}

	[RelayCommand]
    private async Task HideFromDashboardAsync()
    {
		// TODO
		await Task.Delay(1);
    }
}
