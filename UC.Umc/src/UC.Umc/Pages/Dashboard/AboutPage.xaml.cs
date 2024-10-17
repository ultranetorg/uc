using CommunityToolkit.Mvvm.DependencyInjection;
using UC.Umc.ViewModels.Dashboard;
using UC.Umc.Views;

namespace UC.Umc.Pages.Dashboard;

public partial class AboutPage : CustomPage
{
	public AboutPage()
	{
		InitializeComponent();
		BindingContext = Ioc.Default.GetService<AboutViewModel>();
	}

	public AboutPage(AboutViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}
