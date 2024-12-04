using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Uuc.Services;

namespace Uuc.PageModels.Popups;

public partial class CreateAccountPopupModel
(
	IPopupService popupService,
	IAccountsService accountsService

) : ObservableObject
{
	[ObservableProperty]
	private string _name = string.Empty;

	[RelayCommand]
	private async Task Create_OnClicked()
	{
		if (string.IsNullOrEmpty(Name))
		{
			return;
		}
		bool isExists = await accountsService.AnyByName(Name);
		if (isExists)
		{
			return;
		}

		await accountsService.CreateAsync(Name);

		MainThread.BeginInvokeOnMainThread(() => popupService.ClosePopup());
	}
}
