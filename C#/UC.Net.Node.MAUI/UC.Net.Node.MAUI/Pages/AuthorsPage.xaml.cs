namespace UC.Net.Node.MAUI.Pages;

public partial class AuthorsPage : CustomPage
{
    public AuthorsPage()
    {
        InitializeComponent();
        BindingContext = App.ServiceProvider.GetService<AuthorsViewModel>();
    }

    public AuthorsPage(AuthorsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
