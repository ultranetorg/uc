namespace UC.Umc.ViewModels;

public partial class DeleteAccountViewModel : BaseAccountViewModel
{
	// will be splitted into 3 services
	private readonly IServicesMockData _service;

	[ObservableProperty]
    private CustomCollection<Author> _authors = new();

	[ObservableProperty]
    private CustomCollection<Product> _products = new();

    public DeleteAccountViewModel(IServicesMockData service, ILogger<DeleteAccountViewModel> logger) : base(logger)
    {
		_service = service;
		Initialize();
    }

    public override void ApplyQueryAttributes(IDictionary<string, object> query)
	{
        try
        {
            InitializeLoading();

            Account = (AccountViewModel)query[nameof(AccountViewModel)];
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
        await DeleteAccountPopup.Show(Account);
    }

	private void Initialize()
	{
		Authors.Clear();
		Products.Clear();

		Authors.AddRange(_service.Authors);
		Products.AddRange(_service.Products);
	}
}