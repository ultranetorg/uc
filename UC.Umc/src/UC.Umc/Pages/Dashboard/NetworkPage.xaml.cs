using CommunityToolkit.Mvvm.DependencyInjection;
using UC.Umc.ViewModels.Dashboard;
using UC.Umc.Views;

namespace UC.Umc.Pages.Dashboard;

public partial class NetworkPage : CustomPage
{
	public NetworkPage()
	{
		InitializeComponent();
		BindingContext = Ioc.Default.GetService<NetworkViewModel>();
	}

	public NetworkPage(NetworkViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}
