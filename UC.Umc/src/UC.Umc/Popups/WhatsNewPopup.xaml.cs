using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.DependencyInjection;
using UC.Umc.ViewModels.Popups;

namespace UC.Umc.Popups;

public partial class WhatsNewPopup : Popup
{
	private WhatsNewPopupViewModel Vm => (BindingContext as WhatsNewPopupViewModel)!;

	public WhatsNewPopup()
	{
		InitializeComponent();
		BindingContext = Ioc.Default.GetService<WhatsNewPopupViewModel>();
		Vm.Popup = this;
	}
}
