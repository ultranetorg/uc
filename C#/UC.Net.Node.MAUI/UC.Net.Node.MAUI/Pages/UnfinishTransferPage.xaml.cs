namespace UC.Net.Node.MAUI.Pages;

public partial class UnfinishTransferPage : CustomPage
{
    public UnfinishTransferPage()
    {
        InitializeComponent();
        BindingContext = new UnfinishTransferViewModel(ServiceHelper.GetService<ILogger<UnfinishTransferViewModel>>());
    }
}
