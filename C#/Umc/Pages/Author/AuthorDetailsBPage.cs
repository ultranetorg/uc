namespace UC.Umc.Pages;

public partial class AuthorDetailsBPage : CustomPage
{
    public AuthorDetailsBPage()
    {
        InitializeComponent();
        var vm = Ioc.Default.GetService<AuthorDetailsBViewModel>();
		vm.Author = new AuthorViewModel();
        BindingContext = vm;
    }

    public AuthorDetailsBPage(AuthorViewModel author, AuthorDetailsBViewModel vm)
    {
        InitializeComponent();
		vm.Author = author;
        BindingContext = vm;
    }
}
