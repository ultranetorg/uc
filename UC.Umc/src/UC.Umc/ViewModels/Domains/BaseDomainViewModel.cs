using UC.Umc.Common.Constants;
using UC.Umc.Common.Helpers;
using UC.Umc.Models;
using UC.Umc.Popups;

namespace UC.Umc.ViewModels.Domains;

public abstract partial class BaseDomainViewModel(ILogger logger) : BaseViewModel(logger)
{
	[ObservableProperty]
	[NotifyPropertyChangedFor(nameof(CanRegister))]
	[NotifyPropertyChangedFor(nameof(IsMyAuthor))]
	[NotifyPropertyChangedFor(nameof(IsOnAuction))]
	[NotifyPropertyChangedFor(nameof(WatchAuthorText))]
	[NotifyPropertyChangedFor(nameof(HasOwner))]
	private DomainModel _domain;

	[ObservableProperty]
	private AccountModel _account;

	[ObservableProperty]
	private int _position;

	public bool? CanRegister => Domain?.Status == AuthorStatus.Free;

	public bool? IsMyAuthor => Domain?.Status == AuthorStatus.Owned;

	public bool? IsReservedAuthor => Domain?.Status == AuthorStatus.Reserved;

	public bool? IsOnAuction => Domain?.Status == AuthorStatus.Auction || Domain?.Status == AuthorStatus.Watched;

	public string WatchAuthorText => Domain?.Status == AuthorStatus.Watched ? "Unwatch" : "Watch";

	public bool? HasOwner => Domain?.Status == AuthorStatus.Owned || Domain?.Status == AuthorStatus.Reserved;


	[RelayCommand]
	private async Task RenewAuthorAsync()
	{
		await Navigation.GoToAsync(Routes.AUTHOR_RENEWAL,
			new Dictionary<string, object>()
		{
			{ QueryKeys.AUTHOR, Domain }
		});
		ClosePopup();
	}
	
	[RelayCommand]
	private async Task RegisterAuthorAsync()
	{
		await Navigation.GoToAsync(Routes.AUTHOR_REGISTRATION,
			new Dictionary<string, object>()
		{
			{ QueryKeys.AUTHOR, Domain }
		});
		ClosePopup();
	}
	
	[RelayCommand]
	private async Task MakeBidAsync()
	{
		await Navigation.GoToAsync(Routes.MAKE_BID,
			new Dictionary<string, object>()
		{
			{ QueryKeys.AUTHOR, Domain }
		});
		ClosePopup();
	}

	[RelayCommand]
	private async Task TransferAuthorAsync()
	{
		await Navigation.GoToAsync(Routes.AUTHOR_TRANSFER,
			new Dictionary<string, object>()
		{
			{ QueryKeys.AUTHOR, Domain }
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
			var popup = new SelectDomainPopup();
			await ShowPopup(popup);
			if (popup.Vm?.SelectedAuthor != null)
			{
				Domain = popup.Vm.SelectedAuthor;
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
