namespace UC.Net.Node.MAUI.Pages;

public partial class ManageAccountsPage : CustomPage
{
    public ManageAccountsPage()
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<ManageAccountsViewModel>();
    }

    public ManageAccountsPage(ManageAccountsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
