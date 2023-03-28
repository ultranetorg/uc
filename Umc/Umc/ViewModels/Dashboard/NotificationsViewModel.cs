namespace UC.Umc.ViewModels;

public partial class NotificationsViewModel : BasePageViewModel
{
	private readonly INotificationsService _service;

	[ObservableProperty]
    private Notification _notificationDetails;

	[ObservableProperty]
    private CustomCollection<Notification> _notifications = new();


	public NotificationsViewModel(INotificationsService service, ILogger<NotificationsViewModel> logger): base(service, logger)
    {
		_service = service;
		Initialize();
    }
	
	public void Initialize()
	{
		Notifications = _service.GetAll();
	}

	[RelayCommand]
	private async Task OpenDetailsAsync(Notification item)
    {
		try
		{
			Guard.IsNotNull(item);

			await ShowPopup(new NotificationPopup(item));
			
			item.IsRead = true;
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "OpenDetailsAsync Exception: {Ex}", ex.Message);
			await ToastHelper.ShowDefaultErrorMessageAsync();
		}
	}

	[RelayCommand]
	private async Task MarkAsReadAsync(Notification item)
    {
		try
		{
			item.IsRead = !item.IsRead;

			await Task.Delay(10);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "OpenDetailsAsync Exception: {Ex}", ex.Message);
			await ToastHelper.ShowDefaultErrorMessageAsync();
		}
	}
}
