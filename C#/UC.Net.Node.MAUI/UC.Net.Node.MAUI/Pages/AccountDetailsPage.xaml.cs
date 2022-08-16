namespace UC.Net.Node.MAUI.Pages;

public partial class AccountDetailsPage : CustomPage
{
    public AccountDetailsPage(Wallet wallet, AccountDetailsViewModel vm)
    {
        InitializeComponent();
		vm.Wallet = wallet;
        BindingContext = vm;
    }
}
