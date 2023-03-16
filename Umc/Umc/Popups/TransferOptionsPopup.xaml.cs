namespace UC.Umc.Popups;

public partial class TransferOptionsPopup : Popup
{
	TransferOptionsViewModel Vm => BindingContext as TransferOptionsViewModel;

    public TransferOptionsPopup()
    {
		InitializeComponent();
		BindingContext = Ioc.Default.GetService<TransferOptionsViewModel>();
		Vm.Popup = this;
    }
}