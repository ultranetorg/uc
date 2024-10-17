using UC.Umc.ViewModels;

namespace UC.Umc.Pages;

public partial class AppShell : Shell
{
	public AppShell(ShellViewModel viewModel)
	{
		InitializeComponent();

		BindingContext = viewModel;
	}
}
