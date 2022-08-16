namespace UC.Net.Node.MAUI.Pages;

public partial class TransactionsPage : CustomPage
{
    public TransactionsPage(TransactionsViewModel vm = null)
    {
        InitializeComponent();
        BindingContext = vm ?? App.ServiceProvider.GetService<TransactionsViewModel>();
    }
}
