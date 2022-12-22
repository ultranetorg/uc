namespace UC.Umc.Pages;

public partial class AuthorDetailsCPage : CustomPage
{
    public AuthorDetailsCPage()
    {
        InitializeComponent();
        var vm = Ioc.Default.GetService<AuthorDetailsCViewModel>();
		vm.Author = new AuthorViewModel();
        BindingContext = vm;
    }

    public AuthorDetailsCPage(AuthorViewModel author, AuthorDetailsCViewModel vm)
    {
        InitializeComponent();
		vm.Author = author;
        BindingContext = vm;
    }
}
