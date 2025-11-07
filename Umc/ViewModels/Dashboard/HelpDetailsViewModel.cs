namespace UC.Umc.ViewModels;

public partial class HelpDetailsViewModel : BasePageViewModel
{
	[ObservableProperty]
	private HelpInfo _helpDetails;

    public HelpDetailsViewModel(INotificationsService notificationService, ILogger<HelpDetailsViewModel> logger) : base(notificationService, logger)
    {
    }

    public override void ApplyQueryAttributes(IDictionary<string, object> query)
	{
        try
        {
            InitializeLoading();

			HelpDetails = (HelpInfo)query[QueryKeys.HELP_INFO];

#if DEBUG
			_logger.LogDebug("ApplyQueryAttributes HelpInfo: {HelpInfo}", HelpDetails);
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
    private async Task CancelAsync()
    {
        await Navigation.PopAsync();
    }
}
