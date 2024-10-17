using CommunityToolkit.Mvvm.DependencyInjection;
using UC.Umc.ViewModels.Domains;
using UC.Umc.Views;

namespace UC.Umc.Pages.Domains;

public partial class DomainsPage : CustomPage
{
	private DomainsViewModel Vm => (BindingContext as DomainsViewModel)!;

	public DomainsPage()
	{
		InitializeComponent();
		BindingContext = Ioc.Default.GetService<DomainsViewModel>();
	}

	public DomainsPage(DomainsViewModel vm)
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
