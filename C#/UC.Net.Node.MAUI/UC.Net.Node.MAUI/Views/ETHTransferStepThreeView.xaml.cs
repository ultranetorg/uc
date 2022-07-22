namespace UC.Net.Node.MAUI.Views;
public partial class ETHTransferStepThreeView : ContentView
{
    ETHTransferStepThreeViewModel _viewModel;
    public ETHTransferStepThreeView()
    {
        InitializeComponent();
        BindingContext = _viewModel = new ETHTransferStepThreeViewModel(ServiceHelper.GetService<ILogger<ETHTransferStepThreeViewModel>>());
    }
}
