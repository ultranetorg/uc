using UC.Umc.Models;
using UC.Umc.Services;

namespace UC.Umc.ViewModels.Popups;

public partial class NotificationViewModel(ILogger<NotificationViewModel> logger) : BaseViewModel(logger)
{
	[ObservableProperty]
	private Notification _notification = DefaultDataMock.CreateNotification(NotificationSeverity.High, NotificationType.ProductOperations);
}
