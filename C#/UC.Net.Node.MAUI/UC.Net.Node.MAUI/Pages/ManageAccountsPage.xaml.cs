namespace UC.Net.Node.MAUI.Pages;

public partial class ManageAccountsPage : CustomPage
{
    public ManageAccountsPage()
    {
        InitializeComponent();
        BindingContext = new ManageAccountsViewModel(ServiceHelper.GetService<ILogger<ManageAccountsViewModel>>());
    }
}
