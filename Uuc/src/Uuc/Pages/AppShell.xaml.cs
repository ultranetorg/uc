using CommunityToolkit.Maui.Core;
using Uuc.PageModels.Popups;
using Uuc.Services;

namespace Uuc.Pages;

public partial class AppShell : Shell
{
	private readonly INavigationService _navigationService;
	private readonly IPopupService _popupService;

	public AppShell(INavigationService navigationService, IPopupService popupService)
	{
		_navigationService = navigationService;
		_popupService = popupService;

		InitializeComponent();
		// InitializeRouting();
	}

	//protected override async void OnHandlerChanged()
	//{
	//	base.OnHandlerChanged();

	//	if (Handler is not null)
	//	{
	//		await _navigationService.InitializeAsync();
	//	}
	//}

	//private static void InitializeRouting()
	//{
	//	//Routing.RegisterRoute("Login", typeof(LoginView));
	//	Routing.RegisterRoute("Filter", typeof(FiltersView));
	//	Routing.RegisterRoute("ViewCatalogItem", typeof(CatalogItemView));
	//	Routing.RegisterRoute("Basket", typeof(BasketView));
	//	Routing.RegisterRoute("Settings", typeof(SettingsView));
	//	Routing.RegisterRoute("OrderDetail", typeof(OrderDetailView));
	//	Routing.RegisterRoute("Checkout", typeof(CheckoutView));
	//}

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
