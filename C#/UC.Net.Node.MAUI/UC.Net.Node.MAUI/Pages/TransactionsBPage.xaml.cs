namespace UC.Net.Node.MAUI.Pages;

public partial class TransactionsBPage : CustomPage
{
    public TransactionsBPage()
    {
        InitializeComponent();
        BindingContext = new TransactionsBViewModel(ServiceHelper.GetService<ILogger<TransactionsBViewModel>>());
    }
}
