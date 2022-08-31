namespace UC.Net.Node.MAUI.Pages;

public partial class TransferCompletePage : CustomPage
{
    public TransferCompletePage()
    {
        InitializeComponent();
        BindingContext = App.ServiceProvider.GetService<TransferCompleteViewModel>();
    }

    public TransferCompletePage(TransferCompleteViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
