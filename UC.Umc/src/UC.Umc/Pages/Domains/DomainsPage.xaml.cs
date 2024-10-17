using CommunityToolkit.Mvvm.DependencyInjection;
using UC.Umc.ViewModels.Domains;
using UC.Umc.Views;

namespace UC.Umc.Pages.Domains;

public partial class DomainsPage : CustomPage
{
	private DomainViewModel Vm => (BindingContext as DomainViewModel)!;

	public DomainsPage()
	{
		InitializeComponent();
		BindingContext = Ioc.Default.GetService<DomainViewModel>();
	}

	public DomainsPage(DomainViewModel vm)
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
