using CommunityToolkit.Mvvm.DependencyInjection;
using UC.Umc.ViewModels.Dashboard;
using UC.Umc.Views;

namespace UC.Umc.Pages.Dashboard;

public partial class SettingsPage : CustomPage
{
	public SettingsPage()
	{
		InitializeComponent();
		BindingContext = Ioc.Default.GetService<SettingsViewModel>();
	}

	public SettingsPage(SettingsViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}
