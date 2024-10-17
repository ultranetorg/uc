using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.DependencyInjection;
using UC.Umc.Models;
using UC.Umc.ViewModels.Popups;

namespace UC.Umc.Popups;

public partial class DomainOptionsPopup : Popup
{
	private DomainOptionsViewModel Vm => (BindingContext as DomainOptionsViewModel)!;

	public DomainOptionsPopup(DomainModel domain)
	{
		InitializeComponent();
		BindingContext = Ioc.Default.GetService<DomainOptionsViewModel>();
		Vm.Domain = domain;
		Vm.Popup = this;
	}
}
