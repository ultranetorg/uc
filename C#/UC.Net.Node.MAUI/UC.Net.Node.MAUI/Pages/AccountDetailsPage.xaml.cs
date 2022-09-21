namespace UC.Net.Node.MAUI.Pages;

public partial class AccountDetailsPage : CustomPage
{
    public AccountDetailsPage()
    {
        InitializeComponent();
        var vm = Ioc.Default.GetService<AccountDetailsViewModel>();
		vm.Wallet = new Wallet();
        BindingContext = vm;
    }

    public AccountDetailsPage(Wallet wallet, AccountDetailsViewModel vm)
    {
        InitializeComponent();
		vm.Wallet = wallet;
        BindingContext = vm;
    }
}
