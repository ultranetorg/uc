namespace UC.Umc.Services;

public interface INotificationsService
{
	int GetCount();
	Severity GetMaxSeverity();
    CustomCollection<Notification> GetAll();
    Task<int> GetCountAsync();
    Task<CustomCollection<Notification>> GetAllAsync();
}
