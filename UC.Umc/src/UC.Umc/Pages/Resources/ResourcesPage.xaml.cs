using CommunityToolkit.Mvvm.DependencyInjection;
using UC.Umc.ViewModels.Resources;
using UC.Umc.Views;

namespace UC.Umc.Pages.Resources;

public partial class ResourcesPage : CustomPage
{
	private ResourceViewModel Vm => (BindingContext as ResourceViewModel)!;

	public ResourcesPage()
	{
		InitializeComponent();
		BindingContext = Ioc.Default.GetService<ResourceViewModel>();
	}

	public ResourcesPage(ResourceViewModel vm)
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
