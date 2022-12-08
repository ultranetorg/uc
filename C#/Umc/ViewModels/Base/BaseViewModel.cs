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

    public virtual void ApplyQueryAttributes(IDictionary<string, object> query)
    {
    }

	public virtual async Task ShowPopup(Popup popup)
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

    protected string GetControlErrorMessage(string nameOfControl)
    {
        var message = string.Empty;

        try
        {
            if (HasErrors)
            {
               Guard.IsNotNullOrWhiteSpace(nameOfControl);

                var errors = GetErrors(nameOfControl)?.Select(x => x.ErrorMessage).ToList();

                if (errors.Count > 0)
                {
                    message = errors[0];
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetControlErrorMessage({Value}) - Error: {Ex} ", nameOfControl, ex.Message);
        }

        return message;
    }
}
