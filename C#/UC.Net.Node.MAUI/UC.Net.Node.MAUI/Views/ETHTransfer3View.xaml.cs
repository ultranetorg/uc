namespace UC.Net.Node.MAUI.Views;

public partial class ETHTransfer3View : ContentView
{
    public ETHTransfer3View(ETHTransfer3ViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    public ETHTransfer3View()
    {
        InitializeComponent();
        BindingContext = App.ServiceProvider.GetService<ETHTransfer3ViewModel>();
    }
}
