namespace UC.Net.Node.MAUI.Views;
public partial class ETHTransferStepTwoView : ContentView
{
    ETHTransferStepTwoViewModel _viewModel;
    public ETHTransferStepTwoView()
    {
        InitializeComponent();
        BindingContext= _viewModel= new ETHTransferStepTwoViewModel(ServiceHelper.GetService<ILogger<ETHTransferStepTwoViewModel>>());
    }
}