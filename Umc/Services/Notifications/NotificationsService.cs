namespace UC.Umc.Services;

public class NotificationsService : INotificationsService
{
	public int GetCount()
	{
		throw new NotImplementedException();
	}

	public Severity GetMaxSeverity()
	{
		throw new NotImplementedException();
	}

    public Task<int> GetCountAsync()
    {
        throw new NotImplementedException();
    }

    public Task<CustomCollection<Notification>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

	public CustomCollection<Notification> GetAll()
	{
		throw new NotImplementedException();
	}
}
