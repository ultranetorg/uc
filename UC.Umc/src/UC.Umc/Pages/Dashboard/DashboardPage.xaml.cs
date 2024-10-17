using CommunityToolkit.Mvvm.DependencyInjection;
using UC.Umc.ViewModels.Dashboard;
using UC.Umc.Views;

namespace UC.Umc.Pages.Dashboard;

public partial class DashboardPage : CustomPage
{
	private DashboardViewModel Vm => (BindingContext as DashboardViewModel)!;

	public DashboardPage()
	{
		InitializeComponent();
		BindingContext = Ioc.Default.GetService<DashboardViewModel>();
	}
	
	public DashboardPage(DashboardViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();

		await Vm.InitializeAsync();
	}
}
