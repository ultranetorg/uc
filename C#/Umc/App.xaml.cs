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

		ShellBaseRoutes.InitializeRouting();
			
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
}
