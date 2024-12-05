using Uuc.Common.Collections;
using Uuc.Common.Constants;
using Uuc.Models;
using Uuc.Models.Accounts;
using Uuc.PageModels.Base;
using Uuc.Services;

namespace Uuc.PageModels;

public class DashboardPageModel
(
	INavigationService navigationService,
	IAccountsService accountsService,
	IDigitalIdentitiesService digitalIdentitiesService,
	IOperationsService operationsService
): BasePageModel(navigationService)
{
	private readonly ObservableCollectionEx<Account> _accounts = new ();
	public IReadOnlyList<Account> Accounts => _accounts;
	private readonly ObservableCollectionEx<DigitalIdentity> _digitalIdentities = new ();
	public IReadOnlyList<DigitalIdentity> DigitalIdentities => _digitalIdentities;
	private readonly ObservableCollectionEx<Operation> _operations = new ();
	public IReadOnlyList<Operation> Operations => _operations;

	private bool _initialized;

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
				var allAccounts = await accountsService.ListAllAsync();
				var allDigitalIdentities = await digitalIdentitiesService.ListAllAsync();
				var accounts = allAccounts?.Take(PageDashboard.AccountsToDisplay);
				var digitalIdentities = allDigitalIdentities?.Take(PageDashboard.DigitalIdentitiesToDisplay);
				var allOperations = await operationsService.ListAllAsync();
				var operations = allOperations?.Take(PageDashboard.OperationsToDisplay);

				if (accounts != null)
				{
					_accounts.ReloadData(accounts);
				}

				if (digitalIdentities != null)
				{
					_digitalIdentities.ReloadData(digitalIdentities);
				}

				if (operations != null)
				{
					_operations.ReloadData(operations);
				}
			});
	}
}
