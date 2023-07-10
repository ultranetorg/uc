namespace UC.Umc.ViewModels;

public partial class DeleteAccountViewModel : BaseAccountViewModel
{
	private readonly IAuthorsService _authorsService;
	private readonly IProductsService _productsService;

	[ObservableProperty]
    private CustomCollection<AuthorViewModel> _authors = new();

	[ObservableProperty]
    private CustomCollection<ProductViewModel> _products = new();

    public DeleteAccountViewModel(INotificationsService notificationService, IAuthorsService authorsService,
		IProductsService productsService, ILogger<DeleteAccountViewModel> logger) : base(notificationService, logger)
    {
		_authorsService = authorsService;
		_productsService = productsService;
    }

    public override void ApplyQueryAttributes(IDictionary<string, object> query)
	{
        try
        {
            InitializeLoading();

            Account = (AccountViewModel)query[QueryKeys.ACCOUNT];
#if DEBUG
            _logger.LogDebug("ApplyQueryAttributes Account: {Account}", Account);
#endif
			Authors = _authorsService.GetAccountAuthors(Account.Address);
			Products = _productsService.GetAccountProducts(Account.Address);
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
    private async Task DeleteAsync()
    {
		try
		{
			await ShowPopup(new DeleteAccountPopup(Account));
			await Navigation.PopAsync();
			await ToastHelper.ShowMessageAsync(Properties.Additional_Strings.Message_Deleted);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "DeleteAsync Exception: {Ex}", ex.Message);
			await ToastHelper.ShowDefaultErrorMessageAsync();
		}
	}
}