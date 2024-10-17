using CommunityToolkit.Mvvm.DependencyInjection;
using UC.Umc.ViewModels.Dashboard;
using UC.Umc.Views;

namespace UC.Umc.Pages.Dashboard;

public partial class HelpPage : CustomPage
{
	public HelpPage()
	{
		InitializeComponent();
		BindingContext = Ioc.Default.GetService<HelpViewModel>();
	}

	public HelpPage(HelpViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}
