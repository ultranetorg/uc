using CommunityToolkit.Mvvm.DependencyInjection;
using UC.Umc.ViewModels.Resources;
using UC.Umc.Views;

namespace UC.Umc.Pages.Resources;

public partial class ResourcesSearchPage : CustomPage
{
	private ResourceSearchViewModel Vm => (BindingContext as ResourceSearchViewModel)!;

	public ResourcesSearchPage()
	{
		InitializeComponent();
		BindingContext = Ioc.Default.GetService<ResourceSearchViewModel>();
	}

	public ResourcesSearchPage(ResourceSearchViewModel vm)
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
