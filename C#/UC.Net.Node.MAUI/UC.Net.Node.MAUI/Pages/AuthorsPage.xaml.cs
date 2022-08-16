namespace UC.Net.Node.MAUI.Pages;

public partial class AuthorsPage : CustomPage
{
    public AuthorsPage(AuthorsViewModel vm = null)
    {
        InitializeComponent();
        BindingContext = vm ?? App.ServiceProvider.GetService<AuthorsViewModel>();
    }
}
