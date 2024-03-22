namespace UC.Umc.Popups;

public partial class DeleteAccountPopup : Popup
{
	DeleteAccountPopupViewModel Vm => BindingContext as DeleteAccountPopupViewModel;

	public DeleteAccountPopup(AccountViewModel account)
    {
        InitializeComponent();
		BindingContext = Ioc.Default.GetService<DeleteAccountPopupViewModel>();
		Vm.Account = account;
		Vm.Popup = this;
	}
} 