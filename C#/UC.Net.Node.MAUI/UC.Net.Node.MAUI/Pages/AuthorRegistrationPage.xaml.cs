namespace UC.Net.Node.MAUI.Pages;

public partial class AuthorRegistrationPage : CustomPage
{
    public AuthorRegistrationPage()
    {
        InitializeComponent();
        BindingContext = new AuthorRegistrationViewModel(this, ServiceHelper.GetService<ILogger<AuthorRegistrationViewModel>>());
    }
}
