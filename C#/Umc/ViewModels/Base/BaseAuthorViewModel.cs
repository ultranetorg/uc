namespace UC.Umc.ViewModels;

public abstract partial class BaseAuthorViewModel : BaseViewModel
{
	[ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanRegister))]
    [NotifyPropertyChangedFor(nameof(IsOwned))]
    [NotifyPropertyChangedFor(nameof(IsOnAuction))]
    [NotifyPropertyChangedFor(nameof(WatchAuthorText))]
    public AuthorViewModel _author;

    public bool? CanRegister => Author?.Status == AuthorStatus.Free;

    public bool? IsOwned => Author?.Status == AuthorStatus.Owned;

    public bool? IsOnAuction => Author?.Status == AuthorStatus.Auction || Author?.Status == AuthorStatus.Watched;

    public string WatchAuthorText => Author?.Status == AuthorStatus.Watched ? "Unwatch" : "Watch";


	protected BaseAuthorViewModel(ILogger logger) : base(logger)
	{
	}

	[RelayCommand]
	private async Task RenewAuthorAsync()
	{
		await Navigation.GoToAsync(ShellBaseRoutes.AUTHOR_RENEWAL,
			new Dictionary<string, object>()
		{
			{ QueryKeys.AUTHOR, Author }
		});
		ClosePopup();
	}
	
	[RelayCommand]
    private async Task RegisterAuthorAsync()
	{
		await Navigation.GoToAsync(ShellBaseRoutes.AUTHOR_REGISTRATION,
			new Dictionary<string, object>()
		{
			{ QueryKeys.AUTHOR, Author }
		});
		ClosePopup();
	}
	
	[RelayCommand]
    private async Task MakeBidAsync()
	{
		await Navigation.GoToAsync(ShellBaseRoutes.MAKE_BID,
			new Dictionary<string, object>()
		{
			{ QueryKeys.AUTHOR, Author }
		});
		ClosePopup();
	}

	[RelayCommand]
    private async Task TransferAuthorAsync()
    {
		// need to add transfer page
		//await Navigation.GoToAsync(ShellBaseRoutes.AUTHOR_REGISTRATION,
		//	new Dictionary<string, object>()
		//{
		//	{ QueryKeys.AUTHOR, Author }
		//});
		//ClosePopup();
		await Task.Delay(10);
    }

	[RelayCommand]
    private async Task WatchAuthorAsync()
    {
		// watch / unwatch
		await Task.Delay(10);
    }
}
