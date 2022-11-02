namespace UC.Umc.Pages;

public partial class TransferCompletePage : CustomPage
{
    public TransferCompletePage()
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<TransferCompleteViewModel>();
    }

    public TransferCompletePage(TransferCompleteViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
