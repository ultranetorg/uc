namespace UC.Umc.ViewModels;

public partial class AuthorDetailsPViewModel : BaseAuthorViewModel
{
    public AuthorDetailsPViewModel(ILogger<AuthorDetailsPViewModel> logger) : base(logger)
    {
    }

    public override void ApplyQueryAttributes(IDictionary<string, object> query)
	{
        try
        {
            InitializeLoading();

            Author = (AuthorViewModel)query[QueryKeys.AUTHOR];
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
	private async Task RegisterAsync() =>
		await Navigation.GoToAsync(nameof(AuthorRegistrationPage),
			new Dictionary<string, object>(){{ QueryKeys.AUTHOR, Author }});
}
