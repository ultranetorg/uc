namespace UC.Net.Node.MAUI.Views;
public partial class ETHTransferStepTwoView : ContentView
{
    public ETHTransferStepTwoView()
    {
        InitializeComponent();
        BindingContext = new ETHTransferStepTwoViewModel(ServiceHelper.GetService<ILogger<ETHTransferStepTwoViewModel>>());
    }
}