using Uuc.PageModels;

namespace Uuc.Pages;

public partial class AppShell : Shell
{
	public AppShell(AppShellModel model)
	{
		InitializeComponent();
		BindingContext = model;
		InitializeRouting();
	}

	//protected override async void OnHandlerChanged()
	//{
	//	base.OnHandlerChanged();

	//	if (Handler is not null)
	//	{
	//		await _navigationService.InitializeAsync();
	//	}
	//}

	private static void InitializeRouting()
	{
		Routing.RegisterRoute(typeof(AccountDetailsPage).ToString(), typeof(AccountDetailsPage));
		//Routing.RegisterRoute("Login", typeof(LoginView));
		//Routing.RegisterRoute("Filter", typeof(FiltersView));
		//Routing.RegisterRoute("ViewCatalogItem", typeof(CatalogItemView));
		//Routing.RegisterRoute("Basket", typeof(BasketView));
		//Routing.RegisterRoute("Settings", typeof(SettingsView));
		//Routing.RegisterRoute("OrderDetail", typeof(OrderDetailView));
		//Routing.RegisterRoute("Checkout", typeof(CheckoutView));
	}
}
