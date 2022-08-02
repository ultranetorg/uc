namespace UC.Net.Node.MAUI.Pages;

public partial class AuthorsPage : CustomPage
{
    public AuthorsPage()
    {
        InitializeComponent();
        BindingContext = new AuthorsViewModel(ServiceHelper.GetService<ILogger<AuthorsViewModel>>());
    }
}
