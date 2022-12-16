namespace UC.Umc.ViewModels;

public partial class AuthorSearchPViewModel : BaseAuthorViewModel
{
    public AuthorSearchPViewModel(ILogger<AuthorSearchPViewModel> logger) : base(logger)
    {
    }

    public override void ApplyQueryAttributes(IDictionary<string, object> query)
	{
        try
        {
            InitializeLoading();

            Author = (Author)query[QueryKeys.AUTHOR];
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
}
