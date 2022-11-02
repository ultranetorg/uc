namespace UC.Umc.Views;

public partial class AuthorRegistration1View : ContentView
{
    public AuthorRegistration1View()
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<AuthorRegistration1ViewModel>();
    }

    public AuthorRegistration1View(AuthorRegistration1ViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
