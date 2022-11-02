namespace UC.Umc.Views;

public partial class AuthorRenewal1View : ContentView
{
    public AuthorRenewal1View()
    {
        InitializeComponent();
        BindingContext = Ioc.Default.GetService<AuthorRenewal1ViewModel>();
    }

    public AuthorRenewal1View(AuthorRenewal1ViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}