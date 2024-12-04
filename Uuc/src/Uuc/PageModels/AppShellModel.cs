using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.Input;
using Uuc.PageModels.Base;
using Uuc.PageModels.Popups;
using Uuc.Services;

namespace Uuc.PageModels;

public partial class AppShellModel
(
	INavigationService navigationService,
	ISessionService sessionService,
	IPopupService popupService
) : BasePageModel(navigationService)
{
	[RelayCommand]
	private async Task Send_OnClicked()
	{
		await popupService.ShowPopupAsync<SendPopupModel>();
	}

	[RelayCommand]
	private async Task Receive_OnClicked()
	{
		await popupService.ShowPopupAsync<ReceivePopupModel>();
	}

	[RelayCommand]
	private async Task Settings_OnClicked()
	{
	}

	[RelayCommand]
	private async Task Exit_OnClicked()
	{
		sessionService.EndSession();
	}
}
