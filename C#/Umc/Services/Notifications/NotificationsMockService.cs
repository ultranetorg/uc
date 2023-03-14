namespace UC.Umc.Services;

public class NotificationsMockService : INotificationsService
{
    private readonly IServicesMockData _data;

    public NotificationsMockService(IServicesMockData mockServiceData)
    {
        _data = mockServiceData;
    }

    public Task<int> GetNotificationsCountAsync()
    {
        int result = _data.Authors.Count;
        return Task.FromResult(result);
    }

    public Task<CustomCollection<Notification>> GetAllAsync()
    {
        CustomCollection<Notification> result = new(_data.Notifications);
        return Task.FromResult(result);
    }
}
