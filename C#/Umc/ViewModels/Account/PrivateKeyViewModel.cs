namespace UC.Umc.ViewModels;

public partial class PrivateKeyViewModel : BaseViewModel
{
	[ObservableProperty]
    private CustomCollection<AuthorViewModel> _authors = new();

	[ObservableProperty]
    private CustomCollection<ProductViewModel> _products = new();
	
	[ObservableProperty]
    private AccountViewModel _account;

    public PrivateKeyViewModel(ILogger<PrivateKeyViewModel> logger) : base(logger)
    { 
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
    private async Task CopyAsync()
    {
        try
        {
			await Clipboard.SetTextAsync(Account.Address);
            await ToastHelper.ShowMessageAsync("Copied to clipboard");
#if DEBUG
            _logger.LogDebug("CopyAsync Address: {Address}", Account.Address);
#endif
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CopyAsync Exception: {Ex}", ex.Message);
            ToastHelper.ShowErrorMessage(_logger);
        }
    }

	[RelayCommand]
    private async Task CancelAsync()
    {
        try
        {
			await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CancelAsync Exception: {Ex}", ex.Message);
            ToastHelper.ShowErrorMessage(_logger);
        }
    }
}
