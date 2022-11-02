using Application = Microsoft.Maui.Controls.Application;
using Microsoft.Maui.Handlers;
using UC.Umc.Constants;

namespace UC.Umc;

public partial class App : Application
{
	public App(IServiceProvider provider)
	{
		InitializeComponent();

        Ioc.Default.ConfigureServices(provider);

		MainPage = new AppShell();

		GlobalAppTheme.Theme = AppTheme.Dark;
		
		GlobalAppTheme.SetTheme();

		InitializeRouting();
			
        // Workaround for AnimatedModal not working on Android: https://github.com/dotnet/maui/issues/8062
        WindowHandler.Mapper.ModifyMapping(nameof(IWindow.Content), OnWorkaround);
	}

    private void OnWorkaround(IWindowHandler arg1, IWindow arg2, Action<IElementHandler, IElement> arg3)
    {
        WindowHandler.MapContent(arg1, arg2);
    }

	private void InitializeRouting()
	{
		Routing.RegisterRoute(ShellBaseRoutes.ACCOUNT_DETAILS, typeof(AccountDetailsPage));
		Routing.RegisterRoute(ShellBaseRoutes.CREATE_ACCOUNT, typeof(CreateAccountPage));
		Routing.RegisterRoute(ShellBaseRoutes.RESTORE_ACCOUNT, typeof(RestoreAccountPage));
		Routing.RegisterRoute(ShellBaseRoutes.DELETE_ACCOUNT, typeof(DeleteAccountPage));
		Routing.RegisterRoute(ShellBaseRoutes.ABOUT, typeof(AboutPage));
	}
}
