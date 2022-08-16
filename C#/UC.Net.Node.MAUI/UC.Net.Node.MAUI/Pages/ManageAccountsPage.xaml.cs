namespace UC.Net.Node.MAUI.Pages;

public partial class ManageAccountsPage : CustomPage
{
    public ManageAccountsPage(ManageAccountsViewModel vm = null)
    {
        InitializeComponent();
        BindingContext = vm ?? App.ServiceProvider.GetService<ManageAccountsViewModel>();
    }
}
