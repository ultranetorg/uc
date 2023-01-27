namespace UC.Umc.ViewModels;

public partial class ProductTransferViewModel : BaseViewModel
{
	[ObservableProperty]
    private AuthorViewModel _author;

	[ObservableProperty]
    private ProductViewModel _product;

	[ObservableProperty]
	private string _commission = "10 UNT ($15)"; // todo: commission calculation

	[ObservableProperty]
    private int _position;

    public ProductTransferViewModel(ILogger<ProductTransferViewModel> logger) : base(logger)
    {
    }

    public override void ApplyQueryAttributes(IDictionary<string, object> query)
	{
        try
        {
            InitializeLoading();

            Product = (ProductViewModel)query[QueryKeys.PRODUCT];
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

	[RelayCommand]
	private async Task NextWorkaroundAsync()
	{
		if (Position == 0)
		{
			// Workaround for this bug: https://github.com/dotnet/maui/issues/9749
			Position = 1;
			Position = 0;
			Position = 1;
		}
		else
		{
			await Navigation.PopAsync();
			await ToastHelper.ShowMessageAsync("Successfully created!");
		}
	}

	[RelayCommand]
	protected void Prev()
	{
		if (Position > 0)
		{
			Position -= 1;
		}
	}
}
