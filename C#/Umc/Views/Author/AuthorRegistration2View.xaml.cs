namespace UC.Net.Node.MAUI.Views;

public partial class AuthorRegistration2View : ContentView
{
    public AuthorRegistration2View()
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<AuthorRegistration2ViewModel>();
    }

    public AuthorRegistration2View(AuthorRegistration2ViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
