namespace UC.Umc.Services;

public class NotificationsMockService : INotificationsService
{
    private readonly IServicesMockData _data;

    public NotificationsMockService(IServicesMockData mockServiceData)
    {
        _data = mockServiceData;
    }

    public int GetCount()
    {
        return _data.Notifications.Count;
    }

    public Severity GetMaxSeverity()
    {
        return _data.Notifications.Select(x => x.Severity).Max();
    }

    public CustomCollection<Notification> GetAll()
    {
        return new CustomCollection<Notification>(_data.Notifications);
    }

    public async Task<int> GetCountAsync()
    {
        int result = _data.Authors.Count;
        return await Task.FromResult(result);
    }

    public async  Task<CustomCollection<Notification>> GetAllAsync()
    {
        CustomCollection<Notification> result = new(_data.Notifications);
        return await Task.FromResult(result);
    }
}
