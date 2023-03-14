namespace UC.Umc.ViewModels;

public partial class DeleteAccountViewModel : BaseAccountViewModel
{
	// will be splitted into 3 services
	private readonly IServicesMockData _service;

	[ObservableProperty]
    private CustomCollection<AuthorViewModel> _authors = new();

	[ObservableProperty]
    private CustomCollection<ProductViewModel> _products = new();

    public DeleteAccountViewModel(IServicesMockData service, ILogger<DeleteAccountViewModel> logger) : base(logger)
    {
		_service = service;
		LoadData();
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
			await ToastHelper.ShowMessageAsync("Successfully deleted!");
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "DeleteAsync Exception: {Ex}", ex.Message);
			await ToastHelper.ShowDefaultErrorMessageAsync();
		}
	}

	private void LoadData()
	{
		Authors.Clear();
		Products.Clear();

		Authors.AddRange(_service.Authors);
		Products.AddRange(_service.Products);
	}
}