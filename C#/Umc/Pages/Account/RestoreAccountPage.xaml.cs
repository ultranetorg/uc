namespace UC.Umc.Pages;

public partial class RestoreAccountPage : CustomPage
{
    public RestoreAccountPage()
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<RestoreAccountViewModel>();
    }

    public RestoreAccountPage(RestoreAccountViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
