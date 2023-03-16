namespace UO.Mobile.UUC.Services.Notifications;

public interface INotificationsService
{
    Task<int> GetNotificationsCountAsync();
}
