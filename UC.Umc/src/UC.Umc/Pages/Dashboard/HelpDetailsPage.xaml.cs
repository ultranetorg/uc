using CommunityToolkit.Mvvm.DependencyInjection;
using UC.Umc.ViewModels.Dashboard;
using UC.Umc.Views;

namespace UC.Umc.Pages.Dashboard;

public partial class HelpDetailsPage : CustomPage
{
	public HelpDetailsPage()
	{
		InitializeComponent();
		BindingContext = Ioc.Default.GetService<HelpDetailsViewModel>();
	}

	public HelpDetailsPage(HelpDetailsViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}
