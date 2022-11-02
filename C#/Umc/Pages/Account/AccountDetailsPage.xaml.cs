namespace UC.Net.Node.MAUI.Pages;

public partial class AccountDetailsPage : CustomPage
{
    //public AccountDetailsPage()
    //{
    //    InitializeComponent();
    //    var vm = Ioc.Default.GetService<AccountDetailsViewModel>();
    //    BindingContext = vm;
    //}

    public AccountDetailsPage(AccountViewModel account, AccountDetailsViewModel vm)
    {
        InitializeComponent();
		vm.Account = account;
        BindingContext = vm;
    }
}
