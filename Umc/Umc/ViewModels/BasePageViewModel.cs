namespace UC.Umc.ViewModels;

public abstract partial class BasePageViewModel : BaseViewModel, IQueryAttributable
{
	private readonly INotificationsService _notificationService;
		
	[ObservableProperty]
    private string _title = string.Empty;
	
	[ObservableProperty]
    private int _notificationsCount;
	
	[ObservableProperty]
    private int _notificationsSeverity;

	protected BasePageViewModel(INotificationsService notificationService, ILogger logger) : base(logger)
	{
		_notificationService = notificationService;
		LoadNotificationsData();
	}

    public virtual void ApplyQueryAttributes(IDictionary<string, object> query)
    {
    }

	protected void LoadNotificationsData()
	{
		NotificationsCount = _notificationService.GetCount();
		NotificationsSeverity = (int)_notificationService.GetMaxSeverity();
	}
}
