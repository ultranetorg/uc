namespace UC.Umc.Popups;

public partial class WhatsNewPopup : Popup
{
	WhatsNewPopupViewModel Vm => BindingContext as WhatsNewPopupViewModel;

    public WhatsNewPopup()
    {
        InitializeComponent();
		BindingContext = Ioc.Default.GetService<WhatsNewPopupViewModel>();
		Vm.Popup = this;
    }
}