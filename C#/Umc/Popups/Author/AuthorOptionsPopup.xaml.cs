namespace UC.Umc.Popups;

public partial class AuthorOptionsPopup : Popup
{
	AccountOptionsViewModel Vm => BindingContext as AccountOptionsViewModel;

    public AuthorOptionsPopup(AccountViewModel author)
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<AccountOptionsViewModel>();
		Vm.Account = author;
		Vm.Popup = this;
    }
}