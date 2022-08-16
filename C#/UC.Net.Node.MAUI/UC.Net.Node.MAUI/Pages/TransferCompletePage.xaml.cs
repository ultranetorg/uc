namespace UC.Net.Node.MAUI.Pages;

public partial class TransferCompletePage : CustomPage
{
    public TransferCompletePage(TransferCompleteViewModel vm = null)
    {
        InitializeComponent();
        BindingContext = vm ?? App.ServiceProvider.GetService<TransferCompleteViewModel>();
    }
}
