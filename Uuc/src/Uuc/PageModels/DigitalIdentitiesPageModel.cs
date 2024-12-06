using CommunityToolkit.Mvvm.Input;
using Uuc.Common.Collections;
using Uuc.Models;
using Uuc.PageModels.Base;
using Uuc.Pages;
using Uuc.Services;

namespace Uuc.PageModels;

public partial class DigitalIdentitiesPageModel
(
	INavigationService navigationService,
	IDigitalIdentitiesService digitalIdentitiesService
) : BasePageModel(navigationService)
{
	private readonly ObservableCollectionEx<DigitalIdentity> _digitalIdentities = new ();
	public IReadOnlyList<DigitalIdentity> DigitalIdentities => _digitalIdentities;

	private bool _initialized;

	[RelayCommand]
	private async Task Create_OnPressed()
	{
		var input = await Application.Current?.MainPage.DisplayPromptAsync("Digital Identity", "Name");
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
				var digitalIdentities = await digitalIdentitiesService.ListAllAsync();
				if (digitalIdentities != null)
				{
					_digitalIdentities.ReloadData(digitalIdentities);
				}
			});
	}

	[RelayCommand]
	private async Task List_OnTapped(DigitalIdentity? digitalIdentity)
	{
		if (digitalIdentity != null)
		{
			await Shell.Current.GoToAsync(typeof(DigitalIdentityDetailsPage) + "?name=" + digitalIdentity.Name);
		}
	}
}
