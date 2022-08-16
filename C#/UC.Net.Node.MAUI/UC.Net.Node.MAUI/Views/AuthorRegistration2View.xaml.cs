namespace UC.Net.Node.MAUI.Views;

public partial class AuthorRegistration2View : ContentView
{
    public AuthorRegistration2View(AuthorRegistration2ViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    public AuthorRegistration2View()
    {
        InitializeComponent();
        BindingContext = App.ServiceProvider.GetService<AuthorRegistration2ViewModel>();
    }
}
