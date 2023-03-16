namespace UC.Umc.Popups;

public partial class AccountOptionsPopup : Popup
{
	AccountOptionsViewModel Vm => BindingContext as AccountOptionsViewModel;

    public AccountOptionsPopup(AccountViewModel account)
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<AccountOptionsViewModel>();
		Vm.Account = account;
		Vm.Popup = this;
    }
}