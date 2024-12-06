using CommunityToolkit.Mvvm.Input;
using Uuc.Common.Collections;
using Uuc.Models;
using Uuc.PageModels.Base;
using Uuc.Services;

namespace Uuc.PageModels;

public partial class AuthorizationsPageModel
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

	[RelayCommand]
	private async Task List_OnTapped(Authorization? authorization)
	{
		if (authorization != null)
		{
			Application.Current?.MainPage.DisplayAlert(authorization.Source, authorization.Source, authorization.Source);
		}
	}
}
