namespace UC.Net.Node.MAUI.Pages;

public partial class UnfinishTransferPage : CustomPage
{
    public UnfinishTransferPage(UnfinishTransferViewModel vm = null)
    {
        InitializeComponent();
        BindingContext = vm ?? App.ServiceProvider.GetService<UnfinishTransferViewModel>();
    }
}
