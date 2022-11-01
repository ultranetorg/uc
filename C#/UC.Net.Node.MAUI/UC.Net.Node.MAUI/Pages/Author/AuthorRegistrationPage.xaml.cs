namespace UC.Net.Node.MAUI.Pages;

public partial class AuthorRegistrationPage : CustomPage
{
    public AuthorRegistrationPage()
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<AuthorRegistrationViewModel>();
    }

    public AuthorRegistrationPage(AuthorRegistrationViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
