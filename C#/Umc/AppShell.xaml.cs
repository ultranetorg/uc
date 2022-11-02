namespace UC.Umc;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();
		
        BindingContext = new ShellViewModel();
	}
}
