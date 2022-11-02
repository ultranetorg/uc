namespace UC.Umc.Views;

public partial class ETHTransfer3View : ContentView
{
    public ETHTransfer3View()
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<ETHTransfer3ViewModel>();
    }

    public ETHTransfer3View(ETHTransfer3ViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
