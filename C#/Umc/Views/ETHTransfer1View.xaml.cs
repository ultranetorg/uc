namespace UC.Umc.Views;

public partial class ETHTransfer1View : ContentView
{
    public ETHTransfer1View()
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<ETHTransfer1ViewModel>();
    }

    public ETHTransfer1View(ETHTransfer1ViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}