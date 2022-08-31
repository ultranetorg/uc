namespace UC.Net.Node.MAUI.Views;

public partial class ETHTransfer2View : ContentView
{
    public ETHTransfer2View()
    {
        InitializeComponent();
        BindingContext = App.ServiceProvider.GetService<ETHTransfer2ViewModel>();
    }

    public ETHTransfer2View(ETHTransfer2ViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}