namespace UC.Umc.ViewModels.Popups;

public partial class AuthorOptionsViewModel : BaseAuthorViewModel
{
	public bool WatchState { get; set; }

	public AuthorOptionsViewModel(INotificationsService notificationService, ILogger<AuthorOptionsViewModel> logger) : base(notificationService, logger)
	{
	}
	
	
	[RelayCommand]
    private void WatchAuthor()
	{
		WatchState = !WatchState;
	}
}
