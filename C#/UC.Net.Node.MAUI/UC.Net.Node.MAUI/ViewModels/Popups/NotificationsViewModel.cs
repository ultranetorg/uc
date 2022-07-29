namespace UC.Net.Node.MAUI.ViewModels.Popups;

public partial class NotificationsViewModel : BaseViewModel
{
	[ObservableProperty]
    private CustomCollection<Notification> _notifications = new();

    public NotificationsViewModel(ILogger<NotificationsViewModel> logger): base(logger)
    {
		AddFakeData();
    }

	private void AddFakeData()
	{
		Notifications.Add(new Notification
        {
            Title= "Today at 16:00",
            Body= "Your application P2P Browser version 1.12.2 successfully deployed to Ultranet network",
            Type= NotificationType.ProductOperations,
            Severity = Severity.High
        });
        Notifications.Add(new Notification
        {
            Title = "Today at 16:00",
            Body = "Your application P2P Browser version 1.12.2 successfully deployed to Ultranet network",
            Type = NotificationType.SystemEvent,
            Severity = Severity.Low
        });
        Notifications.Add(new Notification
        {
            Title = "Today at 16:00",
            Body = "Your application P2P Browser version 1.12.2 successfully deployed to Ultranet network",
            Type = NotificationType.AuthorOperations,
            Severity = Severity.Mid
        });
        Notifications.Add(new Notification
        {
            Title = "Today at 16:00",
            Body = "Your application P2P Browser version 1.12.2 successfully deployed to Ultranet network",
            Type = NotificationType.TokenOperations,
            Severity = Severity.High
        });
        Notifications.Add(new Notification
        {
            Title = "Today at 16:00",
            Body = "Your application P2P Browser version 1.12.2 successfully deployed to Ultranet network",
            Type = NotificationType.Server,
            Severity = Severity.Low
        });
        Notifications.Add(new Notification
        {
            Title = "Today at 16:00",
            Body = "Your application P2P Browser version 1.12.2 successfully deployed to Ultranet network",
            Type = NotificationType.Wallet,
            Severity = Severity.Mid
        });
        Notifications.Add(new Notification
        {
            Title = "Today at 16:00",
            Body = "Your application P2P Browser version 1.12.2 successfully deployed to Ultranet network",
            Type = NotificationType.Server,
            Severity= Severity.High
        });
	}
}