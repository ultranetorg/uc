namespace UC.Umc.Views;

public partial class AuthorRenewal2View : ContentView
{
    public AuthorRenewal2View()
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<AuthorRenewal2ViewModel>();
    }

    public AuthorRenewal2View(AuthorRenewal2ViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
