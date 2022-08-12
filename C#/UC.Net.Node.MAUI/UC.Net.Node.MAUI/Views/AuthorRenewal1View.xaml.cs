namespace UC.Net.Node.MAUI.Views;

public partial class AuthorRenewal1View : ContentView
{
    public AuthorRenewal1View()
    {
        InitializeComponent();
        BindingContext = new AuthorRenewal1ViewModel(ServiceHelper.GetService<ILogger<AuthorRenewal1ViewModel>>());
    }
}