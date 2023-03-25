namespace UC.Umc.ViewModels.Popups;

public partial class NotificationsViewModel : BaseViewModel
{
	private readonly INotificationsService _service;

	[ObservableProperty]
    private Notification _notificationDetails;

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

	[RelayCommand]
	private async Task OpenDetailsAsync(Notification item)
    {
		try
		{
			NotificationDetails = item;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "OpenDetailsAsync Exception: {Ex}", ex.Message);
			await ToastHelper.ShowDefaultErrorMessageAsync();
		}
	}
}
