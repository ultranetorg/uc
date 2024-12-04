using Uuc.Common.Collections;
using Uuc.Models;
using Uuc.PageModels.Base;
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
}
