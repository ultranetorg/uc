using CommunityToolkit.Mvvm.DependencyInjection;
using UC.Umc.ViewModels.Domains;
using UC.Umc.Views;

namespace UC.Umc.Pages.Domains;

public partial class DomainTransferPage : CustomPage
{
	public DomainTransferPage()
	{
		InitializeComponent();
		BindingContext = Ioc.Default.GetService<DomainTransferViewModel>();
	}

	public DomainTransferPage(DomainTransferViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}
