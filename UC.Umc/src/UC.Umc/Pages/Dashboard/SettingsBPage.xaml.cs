using CommunityToolkit.Mvvm.DependencyInjection;
using UC.Umc.ViewModels.Dashboard;
using UC.Umc.Views;

namespace UC.Umc.Pages.Dashboard;

public partial class SettingsBPage : CustomPage
{
	public SettingsBPage()
	{
		InitializeComponent();
		BindingContext = Ioc.Default.GetService<SettingsBViewModel>();
	}

	public SettingsBPage(SettingsBViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}
