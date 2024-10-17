using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.DependencyInjection;
using UC.Umc.ViewModels.Popups;

namespace UC.Umc.Popups;

public partial class SelectDomainPopup : Popup
{
	public SelectDomainViewModel Vm => (BindingContext as SelectDomainViewModel)!;

	public SelectDomainPopup()
	{
		InitializeComponent();
		BindingContext = Ioc.Default.GetService<SelectDomainViewModel>();
		Vm.Popup = this;
	}
}
