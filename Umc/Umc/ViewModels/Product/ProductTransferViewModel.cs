namespace UC.Umc.ViewModels;

public partial class ProductTransferViewModel : BasePageViewModel
{
	[ObservableProperty]
    private AuthorViewModel _author;

	[ObservableProperty]
    private ProductViewModel _product;

	[ObservableProperty]
	private string _commission = "10 UNT ($15)"; // todo: commission calculation

	[ObservableProperty]
    private int _position;

    public ProductTransferViewModel(INotificationsService notificationService, ILogger<ProductTransferViewModel> logger) : base(notificationService, logger)
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
	private async Task SelectAuthorAsync()
	{
		try
		{
			var popup = new SelectAuthorPopup();
			await ShowPopup(popup);
			if (popup.Vm?.SelectedAuthor != null)
			{
				Author = popup.Vm.SelectedAuthor;
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "SelectAuthorAsync Exception: {Ex}", ex.Message);
		}
	}

	[RelayCommand]
	private async Task NextWorkaroundAsync()
	{
		var isValid = Author != null && Product != null && !string.IsNullOrEmpty(Commission);

		if (Position == 0 && isValid)
		{
			// Workaround for this bug: https://github.com/dotnet/maui/issues/9749
			Position = 1;
			Position = 0;
			Position = 1;
		}
		else if (Position == 1)
		{
			await Navigation.PopAsync();
			await ToastHelper.ShowMessageAsync("Successfully transfered!");
		}
	}

	[RelayCommand]
	protected async Task PrevAsync()
	{
		if (Position > 0)
		{
			Position -= 1;
		}
		else
		{
			await Navigation.PopAsync();
		}
	}
}
