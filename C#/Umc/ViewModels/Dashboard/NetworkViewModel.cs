namespace UC.Umc.ViewModels;

public partial class NetworkViewModel : BaseViewModel
{
	[ObservableProperty]
    private NetworkInfo _networkInfo = DefaultDataMock.NetworkInfo;

    public string CurrentTime => DateTime.UtcNow.ToString("dd MMM yyyy HH:mm");

    public NetworkViewModel(ILogger<NetworkViewModel> logger) : base(logger)
    {
    }

	[RelayCommand]
    private async Task CancelAsync() => await Navigation.BackToDashboardAsync();
}
