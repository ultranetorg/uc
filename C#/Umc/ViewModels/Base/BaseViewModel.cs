namespace UC.Umc.ViewModels;

public abstract partial class BaseViewModel : ObservableValidator, IQueryAttributable
{
	internal Popup Popup { get; set; }

	protected readonly ILogger _logger;

    protected bool IsModalOpen { get; set; }

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

	public async Task ShowPopup(Popup popup)
	{
		Popup = popup;
		await App.Current.MainPage.ShowPopupAsync(popup).ConfigureAwait(false);
	}

	public void ClosePopup()
	{
		if(Popup != null)
		{
			Popup.Close();
		}
	}

    protected async Task OpenModalAsync<TModalPage>() where TModalPage : Page
    {
        try
        {
            IsModalOpen = true;
            await Navigation.OpenModalAsync<TModalPage>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OpenModalAsync Error: {Ex}", ex.Message);
            await ToastHelper.ShowDefaultErrorMessageAsync();
            IsModalOpen = false;
        }
    }
}
