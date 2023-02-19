namespace UC.Umc.Popups;

public partial class AccountColorPopup : Popup
{
	internal AccountColorViewModel Vm => BindingContext as AccountColorViewModel;

    public AccountColorPopup()
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<AccountColorViewModel>();
		Vm.Popup = this;
    }
}