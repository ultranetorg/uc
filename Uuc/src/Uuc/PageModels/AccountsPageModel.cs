using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.Input;
using Uuc.Common.Collections;
using Uuc.Models.Accounts;
using Uuc.PageModels.Base;
using Uuc.PageModels.Popups;
using Uuc.Pages;
using Uuc.Services;

namespace Uuc.PageModels;

public partial class AccountsPageModel
(
	INavigationService navigationService,
	IPopupService popupService,
	IAccountsService accountsService
) : BasePageModel(navigationService)
{
	private readonly ObservableCollectionEx<Account> _accounts = new ();
	public IReadOnlyList<Account> Accounts => _accounts;

	private bool _initialized;

	[RelayCommand]
	private async Task Create_OnClicked()
	{
		await popupService.ShowPopupAsync<CreateAccountPopupModel>();
		await IsBusyFor(ReloadAccounts);
	}

	[RelayCommand]
	private async Task List_OnTapped(Account? account)
	{
		if (account != null)
		{
			await Shell.Current.GoToAsync(typeof(AccountDetailsPage) + "?address=" + account.Address);
		}
	}

	public override async Task InitializeAsync()
	{
		if (_initialized)
		{
			return;
		}

		_initialized = true;
		await IsBusyFor(ReloadAccounts);
	}

	private async Task ReloadAccounts()
	{
		var accounts = await accountsService.ListAllAsync();
		if (accounts != null)
		{
			_accounts.ReloadData(accounts);
		}
	}
}
