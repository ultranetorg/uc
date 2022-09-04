namespace UC.Net.Node.MAUI.Services.Notifications;

public class NotificationsMockService : INotificationsService
{
    public Task<int> GetNotificationsCountAsync()
    {
        return Task.FromResult(3);
    }
}
