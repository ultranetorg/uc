using CommunityToolkit.Mvvm.ComponentModel;
using Uuc.Common.Extensions;
using Uuc.PageModels.Base;
using Uuc.Services;

namespace Uuc.PageModels;

public partial class DigitalIdentityDetailsPageModel
(
	INavigationService navigationService,
	IDigitalIdentitiesService digitalIdentitiesService
) : BasePageModel(navigationService)
{
	[ObservableProperty]
	private string _name = string.Empty;

	public override async void ApplyQueryAttributes(IDictionary<string, object> query)
	{
		if (query.TryGetString("name", out string? name) && !string.IsNullOrEmpty(name))
		{
			var unescapedName = Uri.UnescapeDataString(name);
			await LoadData(unescapedName);
		}
	}

	private async Task LoadData(string name)
	{
		var digitalIdentity = await digitalIdentitiesService.FindAsync(name);
		Name = digitalIdentity.Name;
	}
}
