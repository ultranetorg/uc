namespace UC.Net.Node.MAUI.Views;

public partial class AuthorRegistration1View : ContentView
{
    public AuthorRegistration1View()
    {
        InitializeComponent();
        BindingContext = new AuthorRegistration1ViewModel(ServiceHelper.GetService<ILogger<AuthorRegistration1ViewModel>>());
    }
}
