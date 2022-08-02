namespace UC.Net.Node.MAUI.Pages;

public partial class TransactionsPage : CustomPage
{
    public TransactionsPage()
    {
        InitializeComponent();
        BindingContext = new TransactionsViewModel(ServiceHelper.GetService<ILogger<TransactionsViewModel>>());
    }
}
