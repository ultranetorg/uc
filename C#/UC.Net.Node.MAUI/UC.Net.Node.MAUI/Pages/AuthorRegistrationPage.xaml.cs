namespace UC.Net.Node.MAUI.Pages;

public partial class AuthorRegistrationPage : CustomPage
{
    public AuthorRegistrationPage(AuthorRegistrationViewModel vm = null)
    {
        InitializeComponent();
        BindingContext = vm ?? App.ServiceProvider.GetService<AuthorRegistrationViewModel>();
    }
}
