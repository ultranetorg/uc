namespace UC.Net.Node.MAUI.ViewModels;

public partial class BaseViewModel : ObservableObject
{
	protected readonly ILogger _logger;

	[ObservableProperty]
    private bool _isBusy = false;
		
	[ObservableProperty]
    private bool _isLoadMore = false;
		
	[ObservableProperty]
    private string _title = string.Empty;

	protected BaseViewModel(ILogger logger)
	{
		_logger = logger;
	}

    protected async Task NavigateToAsync(ShellNavigationState state, IDictionary<string, object> parameters = null)
    {
        try
        {
			await Navigation.NavigateToAsync(state, parameters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ShellNavigateTo Exception: {Ex}", ex.Message);
        }
    }
}
