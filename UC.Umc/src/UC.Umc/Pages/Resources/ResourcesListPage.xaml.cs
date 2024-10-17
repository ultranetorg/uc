using CommunityToolkit.Mvvm.DependencyInjection;
using UC.Umc.ViewModels.Resources;
using UC.Umc.Views;

namespace UC.Umc.Pages.Resources;

public partial class ResourcesListPage : CustomPage
{
	private ResourceListViewModel Vm => (BindingContext as ResourceListViewModel)!;

	public ResourcesListPage()
	{
		InitializeComponent();
		BindingContext = Ioc.Default.GetService<ResourceListViewModel>();
	}

	public ResourcesListPage(ResourceListViewModel vm)
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
