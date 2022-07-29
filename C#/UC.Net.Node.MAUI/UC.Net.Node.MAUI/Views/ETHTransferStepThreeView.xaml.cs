namespace UC.Net.Node.MAUI.Views;
public partial class ETHTransferStepThreeView : ContentView
{
    public ETHTransferStepThreeView()
    {
        InitializeComponent();
        BindingContext = new ETHTransferStepThreeViewModel(ServiceHelper.GetService<ILogger<ETHTransferStepThreeViewModel>>());
    }
}
