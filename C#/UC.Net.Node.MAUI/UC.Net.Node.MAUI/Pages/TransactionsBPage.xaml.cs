namespace UC.Net.Node.MAUI.Pages;

public partial class TransactionsBPage : CustomPage
{
    public TransactionsBPage(TransactionsBViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
