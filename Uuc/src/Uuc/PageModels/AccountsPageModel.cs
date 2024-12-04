using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.Input;
using Uuc.Common.Collections;
using Uuc.Models;
using Uuc.Models.Accounts;
using Uuc.PageModels.Base;
using Uuc.PageModels.Popups;
using Uuc.Services;

namespace Uuc.PageModels;

public partial class AccountsPageModel
(
	INavigationService navigationService,
	IAccountsService accountsService,
	IPopupService popupService
) : BasePageModel(navigationService)
{
	private readonly ObservableCollectionEx<Account> _accounts = new ();
	public IReadOnlyList<Account> Accounts => _accounts;

	private bool _initialized;

	[RelayCommand]
	private async Task Create_OnClicked()
	{
		await popupService.ShowPopupAsync<CreateAccountPopupModel>();
		await IsBusyFor(
			async () =>
			{
				var accounts = await accountsService.ListAllAsync();
				if (accounts != null)
				{
					_accounts.ReloadData(accounts);
				}
			});
	}

	public override async Task InitializeAsync()
	{
		if (_initialized)
		{
			return;
		}

		_initialized = true;
		await IsBusyFor(
			async () =>
			{
				var accounts = await accountsService.ListAllAsync();
				if (accounts != null)
				{
					_accounts.ReloadData(accounts);
				}
			});
	}
}
