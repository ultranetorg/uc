namespace UC.Net.Node.MAUI.Pages;

public partial class RestoreAccountPage : CustomPage
{
    public RestoreAccountPage(RestoreAccountViewModel vm = null)
    {
        InitializeComponent();
        BindingContext = vm ?? App.ServiceProvider.GetService<RestoreAccountViewModel>();
    }
}
