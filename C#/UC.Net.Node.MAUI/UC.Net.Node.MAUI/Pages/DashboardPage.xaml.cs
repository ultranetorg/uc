namespace UC.Net.Node.MAUI.Pages;

public partial class DashboardPage : CustomPage
{
    public DashboardPage()
    {
        InitializeComponent();
        BindingContext = App.ServiceProvider.GetService<DashboardViewModel>();
    }
	
    public DashboardPage(DashboardViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
