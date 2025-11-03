namespace UC.Umc.ViewModels.Popups;

public partial class NotificationViewModel : BaseViewModel
{
	[ObservableProperty]
    private Notification _notification;

    public NotificationViewModel(ILogger<NotificationViewModel> logger) : base(logger)
	{
	}
	

	[RelayCommand]
    public async Task MarkAsReadAsync()
	{
		// Mark As Read
		await Task.Delay(10);
	}
}
