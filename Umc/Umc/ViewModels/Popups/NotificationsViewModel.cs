namespace UC.Umc.ViewModels.Popups;

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
		Notifications = _service.GetAll();
	}

	[RelayCommand]
	private void Close() => ClosePopup();
}
