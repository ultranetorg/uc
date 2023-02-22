namespace UC.Umc.ViewModels;

public abstract partial class BaseAuthorViewModel : BaseViewModel
{
	[ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanRegister))]
    [NotifyPropertyChangedFor(nameof(IsMyAuthor))]
    [NotifyPropertyChangedFor(nameof(IsOnAuction))]
    [NotifyPropertyChangedFor(nameof(WatchAuthorText))]
    [NotifyPropertyChangedFor(nameof(HasOwner))]
    private AuthorViewModel _author;

	[ObservableProperty]
    private AccountViewModel _account;

	[ObservableProperty]
    private int _position;

    public bool? CanRegister => Author?.Status == AuthorStatus.Free;

    public bool? IsMyAuthor => Author?.Status == AuthorStatus.Owned;

    public bool? IsReservedAuthor => Author?.Status == AuthorStatus.Reserved;

    public bool? IsOnAuction => Author?.Status == AuthorStatus.Auction || Author?.Status == AuthorStatus.Watched;

    public string WatchAuthorText => Author?.Status == AuthorStatus.Watched ? "Unwatch" : "Watch";

    public bool? HasOwner => Author?.Status == AuthorStatus.Owned || Author?.Status == AuthorStatus.Reserved;


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
		await Navigation.GoToAsync(ShellBaseRoutes.AUTHOR_TRANSFER,
			new Dictionary<string, object>()
		{
			{ QueryKeys.AUTHOR, Author }
		});
		ClosePopup();
    }

	[RelayCommand]
    private async Task WatchAuthorAsync()
    {
		// watch / unwatch
		await Task.Delay(10);
    }

    [RelayCommand]
    private async Task SelectAuthorAsync()
    {
		try
		{
			var popup = new SelectAuthorPopup();
			await ShowPopup(popup);
			if (popup.Vm?.SelectedAuthor != null)
			{
				Author = popup.Vm.SelectedAuthor;
			}
		}
		catch (Exception ex)
		{
            _logger.LogError(ex, "SelectAuthorAsync Exception: {Ex}", ex.Message);
		}
    }

    [RelayCommand]
    private async Task SelectAccountAsync()
	{
		try
		{
			var popup = new SourceAccountPopup();
			await ShowPopup(popup);
			if (popup.Vm?.Account != null)
			{
				Account = popup.Vm.Account;
			}
		}
		catch (Exception ex)
		{
            _logger.LogError(ex, "SelectAccountAsync Exception: {Ex}", ex.Message);
		}
    }

	[RelayCommand]
	private async Task NextWorkaroundAsync()
	{
		if (Position == 0)
		{
			// Workaround for this bug: https://github.com/dotnet/maui/issues/9749
			Position = 1;
			Position = 0;
			Position = 1;
		}
		else
		{
			await Navigation.PopAsync();
			await ToastHelper.ShowMessageAsync("Success!");
		}
	}

	[RelayCommand]
	protected void Prev()
	{
		if (Position > 0)
		{
			Position -= 1;
		}
	}
}
