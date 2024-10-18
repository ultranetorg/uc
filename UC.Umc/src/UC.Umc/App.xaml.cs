using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Maui.Handlers;
using UC.Umc.Common.Constants;
using UC.Umc.Common.Helpers;
using UC.Umc.Pages;
using UC.Umc.Pages.Account;
using UC.Umc.Pages.Dashboard;
using UC.Umc.Pages.Domains;
using UC.Umc.Pages.Resources;
using UC.Umc.Pages.Transactions;

namespace UC.Umc;

public partial class App : Application
{
	// TODO: review initialization code.
	public App(IServiceProvider provider)
	{
		InitializeComponent();

		Ioc.Default.ConfigureServices(provider);

		var appShell = provider.GetRequiredService<AppShell>();
		MainPage = appShell;

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

	private static void OnWorkaround(IWindowHandler arg1, IWindow arg2, Action<IElementHandler, IElement> arg3)
	{
		WindowHandler.MapContent(arg1, arg2);
	}

	private static void InitializeRouting()
	{
		Routing.RegisterRoute(Routes.ACCOUNT_DETAILS, typeof(AccountDetailsPage));
		Routing.RegisterRoute(Routes.CREATE_ACCOUNT, typeof(CreateAccountPage));
		Routing.RegisterRoute(Routes.RESTORE_ACCOUNT, typeof(RestoreAccountPage));
		Routing.RegisterRoute(Routes.DELETE_ACCOUNT, typeof(DeleteAccountPage));
		Routing.RegisterRoute(Routes.PRIVATE_KEY, typeof(PrivateKeyPage));
		Routing.RegisterRoute(Routes.DOMAIN_DETAILS, typeof(DomainDetailsPage));
		Routing.RegisterRoute(Routes.DOMAIN_REGISTRATION, typeof(DomainRegistrationPage));
		Routing.RegisterRoute(Routes.DOMAIN_RENEWAL, typeof(DomainRenewalPage));
		Routing.RegisterRoute(Routes.DOMAIN_TRANSFER, typeof(DomainTransferPage));
		Routing.RegisterRoute(Routes.RESOURCE_REGISTRATION, typeof(ResourceRegistrationPage));
		Routing.RegisterRoute(Routes.RESOURCE_TRANSFER, typeof(ResourceTransferPage));
		Routing.RegisterRoute(Routes.MAKE_BID, typeof(MakeBidPage));
		Routing.RegisterRoute(Routes.RESOURCE_SEARCH, typeof(ResourcesSearchPage));
		Routing.RegisterRoute(Routes.SEND, typeof(SendPage));
		Routing.RegisterRoute(Routes.COMPLETED_TRANSFERS, typeof(TransferCompletePage));
		Routing.RegisterRoute(Routes.UNFINISHED_TRANSFERS, typeof(UnfinishTransferPage));
		Routing.RegisterRoute(Routes.ABOUT, typeof(AboutPage));
		Routing.RegisterRoute(Routes.HELP_DETAILS, typeof(HelpDetailsPage));
	}
}
