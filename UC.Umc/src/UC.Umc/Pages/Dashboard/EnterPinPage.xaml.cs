using CommunityToolkit.Mvvm.DependencyInjection;
using UC.Umc.ViewModels.Dashboard;
using UC.Umc.Views;

namespace UC.Umc.Pages.Dashboard;

public partial class EnterPinPage : CustomPage
{
	public EnterPinPage()
	{
		InitializeComponent();
		BindingContext = Ioc.Default.GetService<EnterPinViewModel>();
	}

	public EnterPinPage(EnterPinViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}
