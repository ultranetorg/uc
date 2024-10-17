using CommunityToolkit.Mvvm.DependencyInjection;
using UC.Umc.ViewModels.Domains;
using UC.Umc.Views;

namespace UC.Umc.Pages.Domains;

public partial class DomainRegistrationPage : CustomPage
{
	public DomainRegistrationPage()
	{
		InitializeComponent();
		BindingContext = Ioc.Default.GetService<DomainRegistrationViewModel>();
	}

	public DomainRegistrationPage(DomainRegistrationViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}
