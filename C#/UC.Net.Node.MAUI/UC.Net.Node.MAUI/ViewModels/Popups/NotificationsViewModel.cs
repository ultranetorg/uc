namespace UC.Net.Node.MAUI.ViewModels.Popups;

public partial class NotificationsViewModel : BaseViewModel
{
	private readonly INotificationsService _service;

	[ObservableProperty]
    private CustomCollection<Notification> _notifications = new();

    public NotificationsViewModel(INotificationsService service, ILogger<NotificationsViewModel> logger): base(logger)
    {
		_service = service;
		Initialize();
    }
	
	public void Initialize()
	{
		var notifications = Task.Run(async () => await _service.GetAllAsync()).Result;
		Notifications.AddRange(notifications);
	}
}
