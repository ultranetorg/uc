namespace UC.Umc.ViewModels;

public partial class AboutViewModel : BasePageViewModel
{
	[ObservableProperty]
	private string _appVersion = AppInfo.Current.VersionString;

    public AboutViewModel(INotificationsService notificationService, ILogger<AboutViewModel> logger) : base(notificationService, logger)
    {
    }

	[RelayCommand]
    private async Task CancelAsync() => await Navigation.BackToDashboardAsync();
}
