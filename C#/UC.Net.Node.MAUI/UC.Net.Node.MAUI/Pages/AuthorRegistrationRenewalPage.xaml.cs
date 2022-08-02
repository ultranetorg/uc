namespace UC.Net.Node.MAUI.Pages;

public partial class AuthorRegistrationRenewalPage : CustomPage
{
    public AuthorRegistrationRenewalPage()
    {
        InitializeComponent();
        BindingContext = new AuthorRegistrationRenewalViewModel(ServiceHelper.GetService<ILogger<AuthorRegistrationRenewalViewModel>>());
    }
}
