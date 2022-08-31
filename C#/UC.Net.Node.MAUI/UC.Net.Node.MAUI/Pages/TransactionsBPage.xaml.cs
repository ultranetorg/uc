namespace UC.Net.Node.MAUI.Pages;

public partial class TransactionsBPage : CustomPage
{
    public TransactionsBPage()
    {
        InitializeComponent();
        BindingContext = App.ServiceProvider.GetService<TransactionsBViewModel>();
    }

    public TransactionsBPage(TransactionsBViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
