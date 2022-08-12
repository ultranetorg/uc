namespace UC.Net.Node.MAUI.Views;
public partial class ETHTransfer2View : ContentView
{
    public ETHTransfer2View()
    {
        InitializeComponent();
        BindingContext = new ETHTransferStepTwoViewModel(ServiceHelper.GetService<ILogger<ETHTransferStepTwoViewModel>>());
    }
}