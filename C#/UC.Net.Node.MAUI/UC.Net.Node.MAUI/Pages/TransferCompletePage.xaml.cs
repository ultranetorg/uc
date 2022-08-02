namespace UC.Net.Node.MAUI.Pages;

public partial class TransferCompletePage : CustomPage
{
    public TransferCompletePage()
    {
        InitializeComponent();
        BindingContext = new TransferCompleteViewModel(ServiceHelper.GetService<ILogger<TransferCompleteViewModel>>());
    }
}
