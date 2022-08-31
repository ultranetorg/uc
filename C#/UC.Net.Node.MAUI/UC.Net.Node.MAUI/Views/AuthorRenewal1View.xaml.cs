namespace UC.Net.Node.MAUI.Views;

public partial class AuthorRenewal1View : ContentView
{
    public AuthorRenewal1View()
    {
        InitializeComponent();
        BindingContext = App.ServiceProvider.GetService<AuthorRenewal1ViewModel>();
    }

    public AuthorRenewal1View(AuthorRenewal1ViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}