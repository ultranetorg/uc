namespace UC.Umc;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		BindingContext = Ioc.Default.GetService<ShellViewModel>();
	}
}
