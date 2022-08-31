namespace UC.Net.Node.MAUI.Pages;

public partial class TransactionsPage : CustomPage
{
    public TransactionsPage()
    {
        InitializeComponent();
        BindingContext = App.ServiceProvider.GetService<TransactionsViewModel>();
    }

    public TransactionsPage(TransactionsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
