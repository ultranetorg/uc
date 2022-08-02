namespace UC.Net.Node.MAUI.Pages;

public partial class AccountDetailsPage : CustomPage
{
    public AccountDetailsPage(Wallet wallet)
    {
        InitializeComponent();
        BindingContext = new AccountDetailsViewModel(wallet, ServiceHelper.GetService<ILogger<AccountDetailsViewModel>>());
    }
}
