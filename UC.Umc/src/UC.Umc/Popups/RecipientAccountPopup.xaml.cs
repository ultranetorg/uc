using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.DependencyInjection;
using UC.Umc.ViewModels.Popups;
using ZXing.Net.Maui;

namespace UC.Umc.Popups;

public partial class RecipientAccountPopup : Popup
{
	public RecipientAccountViewModel Vm => (BindingContext as RecipientAccountViewModel)!;

	public RecipientAccountPopup()
	{
		InitializeComponent();
		BindingContext = Ioc.Default.GetService<RecipientAccountViewModel>();
		Vm.Popup = this;
	}

	private void CameraBarcodeReaderView_OnBarcodesDetected(object? sender, BarcodeDetectionEventArgs e)
	{
		throw new NotImplementedException();
	}
}
