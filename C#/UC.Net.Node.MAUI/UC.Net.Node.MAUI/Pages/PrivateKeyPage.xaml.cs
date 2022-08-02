namespace UC.Net.Node.MAUI.Pages;

public partial class PrivateKeyPage : CustomPage
{
    public PrivateKeyPage(Wallet wallet)
    {
        InitializeComponent();
        BindingContext = new PrivateKeyViewModel(wallet, ServiceHelper.GetService<ILogger<PrivateKeyViewModel>>());
    }
}
