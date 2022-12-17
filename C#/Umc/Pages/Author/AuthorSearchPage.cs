namespace UC.Umc.Pages;

public partial class AuthorSearchPage : CustomPage
{
    public AuthorSearchPage(AuthorViewModel author, AuthorSearchPViewModel vm)
    {
        InitializeComponent();
		vm.Author = author;
        BindingContext = vm;
    }

    public AuthorSearchPage(AuthorViewModel author)
    {
        InitializeComponent();
		var vm = Ioc.Default.GetService<AuthorSearchPViewModel>();
		vm.Author = author;
        BindingContext = vm;
    }

	public AuthorSearchPage()
	{
        InitializeComponent();
		var vm = Ioc.Default.GetService<AuthorSearchPViewModel>();
		vm.Author = new AuthorViewModel();
        BindingContext = vm;
	}
}
