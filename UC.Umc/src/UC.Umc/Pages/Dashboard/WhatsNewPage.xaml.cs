using CommunityToolkit.Mvvm.DependencyInjection;
using UC.Umc.ViewModels.Dashboard;
using UC.Umc.Views;

namespace UC.Umc.Pages.Dashboard;

public partial class WhatsNewPage : CustomPage
{
	public WhatsNewPage()
	{
		InitializeComponent();
		BindingContext = Ioc.Default.GetService<WhatsNewViewModel>();
	}

	public WhatsNewPage(WhatsNewViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}
