namespace UC.Umc.ViewModels;

public abstract partial class BaseViewModel : ObservableValidator, IQueryAttributable
{
	protected readonly ILogger _logger;

	[ObservableProperty]
    private bool _isLoading = false;
		
	[ObservableProperty]
    private bool _isLoaded = false;

    [ObservableProperty]
    private bool _isRefreshing;
		
	[ObservableProperty]
    private string _title = string.Empty;

	protected BaseViewModel(ILogger logger)
	{
		_logger = logger;
	}

    protected virtual void InitializeLoading()
    {
        IsLoading = true;
        IsLoaded = false;
    }

    protected virtual void FinishLoading()
    {
        IsLoading = false;
        IsRefreshing = false;
    }

    public virtual void ApplyQueryAttributes(IDictionary<string, object> query)
    {
    }

    protected async Task NavigateToAsync(ShellNavigationState state, IDictionary<string, object> parameters = null)
    {
        try
        {
			await Navigation.GoToAsync(state, parameters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ShellNavigateTo Exception: {Ex}", ex.Message);
        }
    }
}
