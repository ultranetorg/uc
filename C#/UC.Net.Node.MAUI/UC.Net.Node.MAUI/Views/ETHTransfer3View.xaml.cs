namespace UC.Net.Node.MAUI.Views;
public partial class ETHTransfer3View : ContentView
{
    public ETHTransfer3View()
    {
        InitializeComponent();
        BindingContext = new ETHTransfer3ViewModel(ServiceHelper.GetService<ILogger<ETHTransfer3ViewModel>>());
    }
}
