namespace UC.Net.Node.MAUI.Views;

public partial class AuthorRenewal2View : ContentView
{
    public AuthorRenewal2View()
    {
        InitializeComponent();
        BindingContext = new AuthorRenewal2ViewModel(ServiceHelper.GetService<ILogger<AuthorRenewal2ViewModel>>());
    }
}
