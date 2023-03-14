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

    public AuthorRegistrationViewModel(ILogger<AuthorRegistrationViewModel> logger) : base(logger)
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
}
