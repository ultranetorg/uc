namespace UC.Net.Node.MAUI.Pages;

public partial class AuthorRenewalPage : CustomPage
{
    public AuthorRenewalPage()
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<AuthorRenewalViewModel>();
    }

    public AuthorRenewalPage(AuthorRenewalViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
