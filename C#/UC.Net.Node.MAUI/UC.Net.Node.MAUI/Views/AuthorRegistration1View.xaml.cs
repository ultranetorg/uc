namespace UC.Net.Node.MAUI.Views;

public partial class AuthorRegistration1View : ContentView
{
    public AuthorRegistration1View()
    {
        InitializeComponent();
        BindingContext = App.ServiceProvider.GetService<AuthorRegistration1ViewModel>();
    }

    public AuthorRegistration1View(AuthorRegistration1ViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
