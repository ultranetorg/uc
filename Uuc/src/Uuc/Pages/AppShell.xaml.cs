using CommunityToolkit.Maui.Core;
using Uuc.Popups;

namespace Uuc.Pages;

public partial class AppShell : Shell
{
	private readonly IPopupService _popupService;

	public AppShell(IPopupService popupService)
	{
		_popupService = popupService;

		InitializeComponent();
	}

	private async void Send_OnClicked(object? sender, EventArgs e)
	{
		await _popupService.ShowPopupAsync<SendPopupModel>();
	}

	private async void Receive_OnClicked(object? sender, EventArgs e)
	{
		await _popupService.ShowPopupAsync<ReceivePopupModel>();
	}

	private async void Settings_OnClicked(object? sender, EventArgs e)
	{

	}
}
