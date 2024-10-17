using CommunityToolkit.Mvvm.DependencyInjection;
using UC.Umc.ViewModels.Domains;
using UC.Umc.Views;

namespace UC.Umc.Pages.Domains;

public partial class DomainRenewalPage : CustomPage
{
	public DomainRenewalPage()
	{
		InitializeComponent();
		BindingContext = Ioc.Default.GetService<DomainRenewalViewModel>();
	}

	public DomainRenewalPage(DomainRenewalViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}
