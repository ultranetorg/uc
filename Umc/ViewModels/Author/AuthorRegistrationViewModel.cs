namespace UC.Umc.ViewModels;

public partial class AuthorRegistrationViewModel : BaseAuthorViewModel
{
	[ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Name))]
	private string _title = string.Empty;

	[ObservableProperty]
	private string _commission = "10 UNT ($15)"; // todo: commission calculation

	public string Name => Title == null
		? string.Empty
		: string.Join("", Title.ToLower().Split(default(string[]), StringSplitOptions.RemoveEmptyEntries));

    public AuthorRegistrationViewModel(INotificationsService notificationService,
		ILogger<AuthorRegistrationViewModel> logger) : base(notificationService, logger)
    {
    }

    public override void ApplyQueryAttributes(IDictionary<string, object> query)
	{
        try
        {
            InitializeLoading();

            Title = ((AuthorViewModel)query[QueryKeys.AUTHOR]).Title;
#if DEBUG
            _logger.LogDebug("ApplyQueryAttributes Title: {Title}", Title);
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
	private async Task NextWorkaroundNewAsync()
	{
		var isValid = Account != null && !string.IsNullOrEmpty(Title) && !string.IsNullOrEmpty(Commission);
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
			await ToastHelper.ShowMessageAsync(Properties.Additional_Strings.Message_Success);
		}
	}

	[RelayCommand]
	protected async Task CancelAsync()
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
