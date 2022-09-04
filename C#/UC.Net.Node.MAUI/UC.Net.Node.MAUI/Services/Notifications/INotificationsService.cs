namespace UC.Net.Node.MAUI.Services.Notifications;

public interface INotificationsService
{
    Task<int> GetNotificationsCountAsync();
}
