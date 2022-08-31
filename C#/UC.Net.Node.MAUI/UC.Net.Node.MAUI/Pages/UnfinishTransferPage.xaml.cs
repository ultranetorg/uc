namespace UC.Net.Node.MAUI.Pages;

public partial class UnfinishTransferPage : CustomPage
{
    public UnfinishTransferPage()
    {
        InitializeComponent();
        BindingContext = App.ServiceProvider.GetService<UnfinishTransferViewModel>();
    }
	
    public UnfinishTransferPage(UnfinishTransferViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
