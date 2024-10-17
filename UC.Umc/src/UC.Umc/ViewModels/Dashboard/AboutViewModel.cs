using UC.Umc.Common.Helpers;

namespace UC.Umc.ViewModels.Dashboard;

public partial class AboutViewModel(ILogger<AboutViewModel> logger) : BaseViewModel(logger)
{
	[RelayCommand]
	private async Task CancelAsync() => await Navigation.BackToDashboardAsync();

	[RelayCommand]
	private async Task SomeActionAsync()
	{
		// Some action, e.g. opening the browser
		await Task.Delay(10);
	}
}
