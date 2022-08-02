namespace UC.Net.Node.MAUI.Pages;

public partial class DashboardPage : CustomPage
{
    public DashboardPage()
    {
        InitializeComponent();
        BindingContext = new DashboardViewModel(ServiceHelper.GetService<ILogger<DashboardViewModel>>());
    }
}
