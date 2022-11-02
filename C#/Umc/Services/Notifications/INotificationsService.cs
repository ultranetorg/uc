namespace UC.Umc.Services;

public interface INotificationsService
{
    Task<int> GetNotificationsCountAsync();
    Task<CustomCollection<Notification>> GetAllAsync();
}
