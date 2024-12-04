using Uuc.Common.Collections;
using Uuc.Models;
using Uuc.PageModels.Base;
using Uuc.Services;

namespace Uuc.PageModels;

public class AuthorizationsPageModel
(
	INavigationService navigationService,
	IAuthorizationsService authorizationsService
) : BasePageModel(navigationService)
{
	private readonly ObservableCollectionEx<Authorization> _authorizations = new();
	public IReadOnlyList<Authorization> Authorizations => _authorizations;

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
				var digitalIdentities = await authorizationsService.ListAllAsync();
				if (digitalIdentities != null)
				{
					_authorizations.ReloadData(digitalIdentities);
				}
			});
	}
}
