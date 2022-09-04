namespace UC.Net.Node.MAUI.Services;

public class NotificationsMockService : INotificationsService
{
    public Task<int> GetNotificationsCountAsync() => Task.FromResult(3);
}
