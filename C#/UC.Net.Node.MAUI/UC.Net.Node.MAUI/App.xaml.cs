namespace UC.Net.Node.MAUI;

public partial class App : Application
{
	public static IServiceProvider ServiceProvider { get; set; }

	public App(IServiceProvider sp)
	{
		InitializeComponent();
		ServiceProvider = sp;

		InitializeRouting();
	}

	private void InitializeRouting()
	{
		MainPage = new AppShell();
		
        // Global Routes (Pages not in Shell XAML) Example
        Routing.RegisterRoute(nameof(AboutPage), typeof(AboutPage));

        // Sub-Page Routes Example
        Routing.RegisterRoute(nameof(HelpPage), typeof(HelpPage));
	}
}
