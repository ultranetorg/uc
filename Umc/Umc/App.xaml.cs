using Application = Microsoft.Maui.Controls.Application;

namespace UC.Umc;

public partial class App : Application
{
	public App(IServiceProvider provider)
	{
		InitializeComponent();

		Ioc.Default.ConfigureServices(provider);

		MainPage = new AppShell();
		
		SetTheme();

		// uncomment to test create pincode popup
		// UserSecureStore.RemoveData(TextConstants.PINCODE_KEY);

		InitializeRouting();
	}

	private static void SetTheme()
	{
		GlobalAppTheme.Theme = AppTheme.Dark;
		GlobalAppTheme.SetTheme();
	}

	private static void InitializeRouting()
	{
		Routing.RegisterRoute(Routes.ACCOUNT_DETAILS, typeof(AccountDetailsPage));
		Routing.RegisterRoute(Routes.CREATE_ACCOUNT, typeof(CreateAccountPage));
		Routing.RegisterRoute(Routes.RESTORE_ACCOUNT, typeof(RestoreAccountPage));
		Routing.RegisterRoute(Routes.DELETE_ACCOUNT, typeof(DeleteAccountPage));
		Routing.RegisterRoute(Routes.PRIVATE_KEY, typeof(PrivateKeyPage));
		Routing.RegisterRoute(Routes.AUTHOR_DETAILS, typeof(AuthorDetailsPage));
		Routing.RegisterRoute(Routes.AUTHOR_REGISTRATION, typeof(AuthorRegistrationPage));
		Routing.RegisterRoute(Routes.AUTHOR_RENEWAL, typeof(AuthorRenewalPage));
		Routing.RegisterRoute(Routes.AUTHOR_TRANSFER, typeof(AuthorTransferPage));
		Routing.RegisterRoute(Routes.PRODUCT_REGISTRATION, typeof(ProductRegistrationPage));
		Routing.RegisterRoute(Routes.PRODUCT_TRANSFER, typeof(ProductTransferPage));
		Routing.RegisterRoute(Routes.MAKE_BID, typeof(MakeBidPage));
		Routing.RegisterRoute(Routes.PRODUCT_SEARCH, typeof(ProductSearchPage));
		Routing.RegisterRoute(Routes.SEND, typeof(SendPage));
		Routing.RegisterRoute(Routes.COMPLETED_TRANSFERS, typeof(TransferCompletePage));
		Routing.RegisterRoute(Routes.UNFINISHED_TRANSFERS, typeof(UnfinishTransferPage));
		Routing.RegisterRoute(Routes.HELP_DETAILS, typeof(HelpDetailsPage));
		Routing.RegisterRoute(Routes.NOTIFICATIONS, typeof(NotificationsPage));
		Routing.RegisterRoute(Routes.CREATE_PINCODE, typeof(CreatePinPage));
	}
}
