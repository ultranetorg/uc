namespace UC.Net.Node.MAUI.Pages;

public partial class DashboardPage : CustomPage
{
    public DashboardPage(DashboardViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
