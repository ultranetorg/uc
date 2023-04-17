namespace UC.Umc.Popups;

public partial class CreatePinPopup : Popup
{
	CreatePinViewModel Vm => BindingContext as CreatePinViewModel;

	public CreatePinPopup()
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<CreatePinViewModel>();
		Vm.Popup = this;
	}
}