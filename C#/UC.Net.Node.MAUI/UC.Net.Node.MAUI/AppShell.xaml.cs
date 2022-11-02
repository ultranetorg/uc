namespace UC.Net.Node.MAUI;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		
        BindingContext = new ShellViewModel();
	}
}
