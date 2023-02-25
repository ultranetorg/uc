using Application = Microsoft.Maui.Controls.Application;
using Microsoft.Maui.Handlers;

namespace UC.Umc;

public partial class App : Application
{
	public App(IServiceProvider provider)
	{
		InitializeComponent();

		Ioc.Default.ConfigureServices(provider);

		MainPage = new AppShell();
		
		SetTheme();

		InitializeRouting();
			
		// Workaround for AnimatedModal not working on Android: https://github.com/dotnet/maui/issues/8062
		WindowHandler.Mapper.ModifyMapping(nameof(IWindow.Content), OnWorkaround);
	}

	private static void SetTheme()
	{
		GlobalAppTheme.Theme = AppTheme.Dark;
		GlobalAppTheme.SetTheme();
	}

	private void OnWorkaround(IWindowHandler arg1, IWindow arg2, Action<IElementHandler, IElement> arg3)
	{
		WindowHandler.MapContent(arg1, arg2);
	}

	private static void InitializeRouting()
	{
		Routing.RegisterRoute(ShellBaseRoutes.ACCOUNT_DETAILS, typeof(AccountDetailsPage));
		Routing.RegisterRoute(ShellBaseRoutes.CREATE_ACCOUNT, typeof(CreateAccountPage));
		Routing.RegisterRoute(ShellBaseRoutes.RESTORE_ACCOUNT, typeof(RestoreAccountPage));
		Routing.RegisterRoute(ShellBaseRoutes.DELETE_ACCOUNT, typeof(DeleteAccountPage));
		Routing.RegisterRoute(ShellBaseRoutes.PRIVATE_KEY, typeof(PrivateKeyPage));
		Routing.RegisterRoute(ShellBaseRoutes.AUTHOR_DETAILS, typeof(AuthorDetailsPage));
		Routing.RegisterRoute(ShellBaseRoutes.AUTHOR_REGISTRATION, typeof(AuthorRegistrationPage));
		Routing.RegisterRoute(ShellBaseRoutes.AUTHOR_RENEWAL, typeof(AuthorRenewalPage));
		Routing.RegisterRoute(ShellBaseRoutes.AUTHOR_TRANSFER, typeof(AuthorTransferPage));
		Routing.RegisterRoute(ShellBaseRoutes.PRODUCT_REGISTRATION, typeof(ProductRegistrationPage));
		Routing.RegisterRoute(ShellBaseRoutes.PRODUCT_TRANSFER, typeof(ProductTransferPage));
		Routing.RegisterRoute(ShellBaseRoutes.MAKE_BID, typeof(MakeBidPage));
		Routing.RegisterRoute(ShellBaseRoutes.PRODUCT_SEARCH, typeof(ProductSearchPage));
		Routing.RegisterRoute(ShellBaseRoutes.SEND, typeof(SendPage));
		Routing.RegisterRoute(ShellBaseRoutes.COMPLETED_TRANSFERS, typeof(TransferCompletePage));
		Routing.RegisterRoute(ShellBaseRoutes.UNFINISHED_TRANSFERS, typeof(UnfinishTransferPage));
		Routing.RegisterRoute(ShellBaseRoutes.ABOUT, typeof(AboutPage));
	}
}
