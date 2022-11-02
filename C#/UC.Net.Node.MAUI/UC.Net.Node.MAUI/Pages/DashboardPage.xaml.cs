namespace UC.Net.Node.MAUI.Pages;

public partial class DashboardPage : CustomPage
{
    DashboardViewModel Vm => BindingContext as DashboardViewModel;

    public DashboardPage()
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<DashboardViewModel>();
    }
	
    public DashboardPage(DashboardViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await Vm.InitializeAsync();
    }
}
