namespace UC.Net.Node.MAUI.Pages;

public partial class AuthorRenewalPage : CustomPage
{
    public AuthorRenewalPage()
    {
        InitializeComponent();
        BindingContext = new AuthorRenewalViewModel(ServiceHelper.GetService<ILogger<AuthorRenewalViewModel>>());
    }
}
