namespace UC.Umc.ViewModels.Popups;

public partial class NotificationViewModel : BaseViewModel
{
	[ObservableProperty]
    private Notification _notification;

    public NotificationViewModel(ILogger<NotificationViewModel> logger) : base(logger)
	{
	}
}
