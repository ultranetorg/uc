namespace UC.Net.Node.MAUI;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		MainPage = new AppShell();

		// NEXT Step:
		// Set up Application Shell initialization, add basic navigation

		// MainPage = new NavigationPage(new StartPage());
	}
}
