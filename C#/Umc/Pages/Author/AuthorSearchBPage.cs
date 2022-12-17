namespace UC.Umc.Pages;

public partial class AuthorSearchBPage : CustomPage
{
    public AuthorSearchBPage()
    {
        InitializeComponent();
        var vm = Ioc.Default.GetService<AuthorSearchBViewModel>();
		vm.Author = new AuthorViewModel();
        BindingContext = vm;
    }

    public AuthorSearchBPage(AuthorViewModel author, AuthorSearchBViewModel vm)
    {
        InitializeComponent();
		vm.Author = author;
        BindingContext = vm;
    }
}
