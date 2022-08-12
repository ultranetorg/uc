namespace UC.Net.Node.MAUI;

public partial class App : Application
{
	public static IServiceProvider ServiceProvider { get; set; }

	public App(IServiceProvider sp)
	{
		InitializeComponent();
		ServiceProvider = sp;
		
		MainPage = ServiceProvider.GetService<AppShell>();

		// NEXT Step:
		// Set up Application Shell initialization, add basic navigation

		// MainPage = new NavigationPage(new StartPage());
	}
}
