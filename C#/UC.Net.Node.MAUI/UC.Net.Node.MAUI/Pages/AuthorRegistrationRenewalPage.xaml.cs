namespace UC.Net.Node.MAUI.Pages;

public partial class AuthorRegistrationRenewalPage : CustomPage
{
    public AuthorRegistrationRenewalPage()
    {
        InitializeComponent();
        BindingContext = new AuthorRegistrationRenewalViewModel(this, ServiceHelper.GetService<ILogger<AuthorRegistrationRenewalViewModel>>());
    }
}
