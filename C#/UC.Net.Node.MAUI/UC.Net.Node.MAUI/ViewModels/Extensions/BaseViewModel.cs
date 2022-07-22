namespace UC.Net.Node.MAUI.ViewModels;
public partial class BaseViewModel : ObservableObject
{
	protected readonly ILogger _logger;

	[ObservableProperty]
    private bool _isBusy = false;
		
	[ObservableProperty]
    private bool _isLoadMore = false;
		
	[ObservableProperty]
    private string _title = string.Empty;

	protected BaseViewModel(ILogger logger)
	{
		_logger = logger;
	}
}
