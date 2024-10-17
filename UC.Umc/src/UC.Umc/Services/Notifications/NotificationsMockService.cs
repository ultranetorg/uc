using System.Collections.ObjectModel;
using UC.Umc.Models;

namespace UC.Umc.Services.Notifications;

public class NotificationsMockService(IServicesMockData mockServiceData) : INotificationsService
{
	public Task<int> GetNotificationsCountAsync()
	{
		int result = mockServiceData.Domains.Count;
		return Task.FromResult(result);
	}

	public Task<ObservableCollection<Notification>> GetAllAsync()
	{
		ObservableCollection<Notification> result = new(mockServiceData.Notifications);
		return Task.FromResult(result);
	}
}
