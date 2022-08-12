namespace UC.Net.Node.MAUI.Views;

public partial class AuthorRegistration2View : ContentView
{
    public AuthorRegistration2View()
    {
        InitializeComponent();
        BindingContext = new AuthorRegistration2ViewModel(ServiceHelper.GetService<ILogger<AuthorRegistration2ViewModel>>());
    }
}
