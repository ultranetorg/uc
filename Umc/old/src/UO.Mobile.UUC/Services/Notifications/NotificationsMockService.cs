namespace UO.Mobile.UUC.Services.Notifications;

public class NotificationsMockService : INotificationsService
{
    public Task<int> GetNotificationsCountAsync()
    {
        return Task.FromResult(3);
    }
}
