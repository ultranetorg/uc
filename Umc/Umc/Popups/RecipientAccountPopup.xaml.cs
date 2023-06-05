using ZXing.Net.Maui;

namespace UC.Umc.Popups;

public partial class RecipientAccountPopup : Popup
{
	public RecipientAccountViewModel Vm => BindingContext as RecipientAccountViewModel;

    public RecipientAccountPopup()
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<RecipientAccountViewModel>();
		Vm.Popup = this;
    }

	private void cameraBarcodeReaderView_BarcodesDetected(object sender, BarcodeDetectionEventArgs e)
	{
		//
	}
}