namespace UC.Net.Node.MAUI.Views;

public partial class AuthorRenewal2View : ContentView
{
    public AuthorRenewal2View(AuthorRenewal2ViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    public AuthorRenewal2View()
    {
        InitializeComponent();
        BindingContext = App.ServiceProvider.GetService<AuthorRenewal2ViewModel>();
    }
}
