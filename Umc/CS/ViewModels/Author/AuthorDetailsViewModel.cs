namespace UC.Umc.ViewModels;

public partial class AuthorDetailsViewModel : BaseAuthorViewModel
{
	private readonly IServicesMockData _service;
	
	[ObservableProperty]
    private CustomCollection<ProductViewModel> _products = new();
	
	[ObservableProperty]
    private CustomCollection<Bid> _bidsHistory = new();

    public AuthorDetailsViewModel(IServicesMockData service, ILogger<AuthorDetailsViewModel> logger) : base(logger)
    {
		_service = service;
		LoadData();
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

	private void LoadData()
	{
		Products.Clear();
		BidsHistory.Clear();

		Products.AddRange(_service.Products);
		BidsHistory.AddRange(_service.BidsHistory);
		
		// TODO: add form object, the account is coming from api
	}

	[RelayCommand]
    private async Task HideFromDashboardAsync()
    {
		// TODO
		await Task.Delay(1);
    }
}
