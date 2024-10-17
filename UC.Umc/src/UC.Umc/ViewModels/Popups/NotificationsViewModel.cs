using System.Collections.ObjectModel;
using UC.Umc.Models;
using UC.Umc.Services.Notifications;

namespace UC.Umc.ViewModels.Popups;

public partial class NotificationsViewModel : BaseViewModel
{
	private readonly INotificationsService _service;

	[ObservableProperty]
	private ObservableCollection<Notification> _notifications = new();

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
