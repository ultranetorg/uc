namespace UC.Net.Node.MAUI.Services;

public interface INotificationsService
{
    Task<int> GetNotificationsCountAsync();
    Task<CustomCollection<Notification>> GetAllAsync();
}
