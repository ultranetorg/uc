using System.Collections.ObjectModel;
using UC.Umc.Models;

namespace UC.Umc.Services.Notifications;

public class NotificationsService : INotificationsService
{
	public Task<int> GetNotificationsCountAsync()
	{
		throw new NotImplementedException();
	}

	public Task<ObservableCollection<Notification>> GetAllAsync()
	{
		throw new NotImplementedException();
	}
}
