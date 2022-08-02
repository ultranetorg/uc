namespace UC.Net.Node.MAUI.Pages;

public partial class DeleteAccountPage : CustomPage
{
    public DeleteAccountPage(Wallet wallet)
    {
        InitializeComponent();
        BindingContext = new DeleteAccountViewModel(wallet, ServiceHelper.GetService<ILogger<DeleteAccountViewModel>>());
    }
}

