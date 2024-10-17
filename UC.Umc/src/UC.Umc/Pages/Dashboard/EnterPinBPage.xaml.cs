using CommunityToolkit.Mvvm.DependencyInjection;
using UC.Umc.ViewModels.Dashboard;
using UC.Umc.Views;

namespace UC.Umc.Pages.Dashboard;

public partial class EnterPinBPage : CustomPage
{
	public EnterPinBPage()
	{
		InitializeComponent();
		BindingContext = Ioc.Default.GetService<EnterPinBViewModel>();
	}

	public EnterPinBPage(EnterPinBViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}
