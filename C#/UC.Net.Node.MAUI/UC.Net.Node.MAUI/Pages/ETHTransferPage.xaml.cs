namespace UC.Net.Node.MAUI.Pages;

public partial class ETHTransferPage : CustomPage
{
    public ETHTransferPage()
    {
        InitializeComponent();
        BindingContext = new ETHTransferViewModel(ServiceHelper.GetService<ILogger<ETHTransferViewModel>>());
    }
}
