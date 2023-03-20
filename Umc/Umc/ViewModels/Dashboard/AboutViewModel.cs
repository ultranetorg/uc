namespace UC.Umc.ViewModels;

public partial class AboutViewModel : BasePageViewModel
{
    public AboutViewModel(INotificationsService notificationService, ILogger<AboutViewModel> logger) : base(notificationService, logger)
    {
    }

	[RelayCommand]
    private async Task CancelAsync() => await Navigation.BackToDashboardAsync();

	[RelayCommand]
    private async Task SomeActionAsync()
    {
		// Some action, e.g. opening the browser
        await Task.Delay(10);
    }
}
