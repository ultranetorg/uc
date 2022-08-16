namespace UC.Net.Node.MAUI.Pages;

public partial class ETHTransferPage : CustomPage
{
    public ETHTransferPage(ETHTransferViewModel vm = null)
    {
        InitializeComponent();
        BindingContext = vm ?? App.ServiceProvider.GetService<ETHTransferViewModel>();
    }
}
