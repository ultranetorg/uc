namespace UC.Net.Node.MAUI.ViewModels.Popups;

public partial class NotificationViewModel : BaseViewModel
{
	[ObservableProperty]
    private Notification _notification = DefaultDataMock.CreateNotification(Severity.High, NotificationType.ProductOperations);

    public NotificationViewModel(ILogger<NotificationViewModel> logger): base(logger){}
}
