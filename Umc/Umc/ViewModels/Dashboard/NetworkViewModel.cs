namespace UC.Umc.ViewModels;

public partial class NetworkViewModel : BasePageViewModel
{
	[ObservableProperty]
    private NetworkInfo _networkInfo = DefaultDataMock.NetworkInfo;

    public string CurrentTime => CommonHelper.GetTodayDate;

    public NetworkViewModel(INotificationsService notificationService,
		ILogger<NetworkViewModel> logger) : base(notificationService, logger)
    {
    }

	[RelayCommand]
    private async Task CancelAsync() => await Navigation.BackToDashboardAsync();
}
