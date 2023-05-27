namespace UC.Umc.ViewModels;

public partial class AboutViewModel : BasePageViewModel
{
    public AboutViewModel(INotificationsService notificationService, ILogger<AboutViewModel> logger) : base(notificationService, logger)
    {
    }

	[RelayCommand]
    private async Task CancelAsync() => await Navigation.BackToDashboardAsync();
}
