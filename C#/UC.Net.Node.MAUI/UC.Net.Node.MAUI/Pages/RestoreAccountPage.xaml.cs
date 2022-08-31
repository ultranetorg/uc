namespace UC.Net.Node.MAUI.Pages;

public partial class RestoreAccountPage : CustomPage
{
    public RestoreAccountPage()
    {
        InitializeComponent();
        BindingContext = App.ServiceProvider.GetService<RestoreAccountViewModel>();
    }

    public RestoreAccountPage(RestoreAccountViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
