namespace UC.Net.Node.MAUI.Pages;

public partial class RestoreAccountPage : CustomPage
{
    public RestoreAccountPage()
    {
        InitializeComponent();
        BindingContext = new RestoreAccountViewModel(ServiceHelper.GetService<ILogger<RestoreAccountViewModel>>());
    }
}
