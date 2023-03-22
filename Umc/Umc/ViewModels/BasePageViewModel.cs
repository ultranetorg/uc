namespace UC.Umc.ViewModels;

public abstract partial class BasePageViewModel : BaseViewModel, IQueryAttributable
{
	private readonly INotificationsService _notificationService;
		
	[ObservableProperty]
    private string _title = string.Empty;
	
	[ObservableProperty]
    private NetworkAccess _connectionState;
	
	[ObservableProperty]
    private int _notificationsCount;
	
	[ObservableProperty]
    private Severity _notificationsSeverity;

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
		NotificationsSeverity = _notificationService.GetMaxSeverity();

		Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
		ConnectionState = CommonHelper.CheckConnectivity();
	}

	void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
	{
		if (e.NetworkAccess == NetworkAccess.ConstrainedInternet)
			_logger.LogWarning("Internet access is available but is limited.");

		else if (e.NetworkAccess != NetworkAccess.Internet)
			_logger.LogWarning("Internet access has been lost.");

		ConnectionState = e.NetworkAccess;
		// ToastHelper.ShowMessageAsync(ConnectionState.ToString());
	}

	~BasePageViewModel() => Connectivity.ConnectivityChanged -= Connectivity_ConnectivityChanged;
}
