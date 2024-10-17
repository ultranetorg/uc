using UC.Umc.Common.Helpers;
using UC.Umc.Models;
using UC.Umc.Services;

namespace UC.Umc.ViewModels.Dashboard;

public partial class NetworkViewModel(ILogger<NetworkViewModel> logger) : BaseViewModel(logger)
{
	[ObservableProperty]
	private NetworkInfo _networkInfo = DefaultDataMock.NetworkInfo;

	public string CurrentTime => DateTime.UtcNow.ToString("dd MMM yyyy HH:mm");

	[RelayCommand]
	private async Task CancelAsync() => await Navigation.BackToDashboardAsync();
}
