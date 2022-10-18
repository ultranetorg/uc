namespace UC.Net.Node.MAUI.Pages;

public partial class CreateAccountPage : CustomPage
{
    public CreateAccountPage()
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<CreateAccountPageViewModel>();
    }

    public CreateAccountPage(CreateAccountPageViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
