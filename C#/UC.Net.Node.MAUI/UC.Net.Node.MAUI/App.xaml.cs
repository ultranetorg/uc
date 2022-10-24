using Application = Microsoft.Maui.Controls.Application;
using Microsoft.Maui.Handlers;

namespace UC.Net.Node.MAUI;

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
        // Global Routes (Pages not in Shell XAML) Example
        Routing.RegisterRoute(nameof(AccountDetailsPage), typeof(AccountDetailsPage));
        Routing.RegisterRoute(nameof(AboutPage), typeof(AboutPage));

        // Sub-Page Routes Example
        // Routing.RegisterRoute(nameof(HelpPage), typeof(HelpPage));
	}
}
