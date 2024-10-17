using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.DependencyInjection;
using UC.Umc.ViewModels.Popups;

namespace UC.Umc.Popups;

public partial class TransferOptionsPopup : Popup
{
	private TransferOptionsViewModel Vm => (BindingContext as TransferOptionsViewModel)!;

	public TransferOptionsPopup()
	{
		InitializeComponent();
		BindingContext = Ioc.Default.GetService<TransferOptionsViewModel>();
		Vm.Popup = this;
	}
}
