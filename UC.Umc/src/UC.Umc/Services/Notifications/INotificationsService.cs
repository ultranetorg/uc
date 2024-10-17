using System.Collections.ObjectModel;
using UC.Umc.Models;

namespace UC.Umc.Services.Notifications;

public interface INotificationsService
{
	Task<int> GetNotificationsCountAsync();
	Task<ObservableCollection<Notification>> GetAllAsync();
}
