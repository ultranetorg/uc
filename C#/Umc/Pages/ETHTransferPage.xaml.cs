namespace UC.Umc.Pages;

public partial class ETHTransferPage : CustomPage
{
    public ETHTransferPage()
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<ETHTransferViewModel>();
    }

    public ETHTransferPage(ETHTransferViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
