namespace UC.Net.Node.MAUI.Pages;

public partial class AccountDetailsPage : CustomPage
{
    public AccountDetailsPage()
    {
        InitializeComponent();
        var vm = Ioc.Default.GetService<AccountDetailsViewModel>();
		vm.Account = DefaultDataMock.Account1;
        BindingContext = vm;
    }

    public AccountDetailsPage(Account account, AccountDetailsViewModel vm)
    {
        InitializeComponent();
		vm.Account = account;
        BindingContext = vm;
    }
}
