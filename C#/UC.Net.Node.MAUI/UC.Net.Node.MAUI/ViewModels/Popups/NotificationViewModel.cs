namespace UC.Net.Node.MAUI.ViewModels.Popups;

public partial class NotificationViewModel : BaseViewModel
{
	[ObservableProperty]
    private Notification _notification = new()
	{
        Title = "Today at 16:00",
        Body = "Your application P2P Browser version 1.12.2 successfully deployed to Ultranet network",
        Type = NotificationType.ProductOperations,
        Severity = Severity.High
    };

    public NotificationViewModel(ILogger<NotificationViewModel> logger): base(logger){}
}
