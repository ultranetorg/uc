namespace UC.Umc.Pages;

public partial class AccountDetailsPage : CustomPage
{
	public AccountDetailsPage()
	{
		InitializeComponent();
		var vm = Ioc.Default.GetService<AccountDetailsViewModel>();
		BindingContext = vm;
	}

	public AccountDetailsPage(AccountViewModel account)
    {
        InitializeComponent();
        var vm = Ioc.Default.GetService<AccountDetailsViewModel>();
		vm.Account = account;
        BindingContext = vm;
    }
}
