namespace UC.Net.Node.MAUI.Pages;

public partial class AuthorRenewalPage : CustomPage
{
    public AuthorRenewalPage(AuthorRenewalViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
