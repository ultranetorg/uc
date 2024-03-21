namespace UC.Umc.ViewModels;

public partial class AuthorTransferViewModel : BaseAuthorViewModel
{
	[ObservableProperty]
	private string _commission = "10 UNT ($15)"; // todo: commission calculation

    public AuthorTransferViewModel(ILogger<AuthorTransferViewModel> logger) : base(logger)
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
}
