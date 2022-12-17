namespace UC.Umc.Pages;

public partial class AuthorSearchCPage : CustomPage
{
    public AuthorSearchCPage()
    {
        InitializeComponent();
        var vm = Ioc.Default.GetService<AuthorSearchCViewModel>();
		vm.Author = new AuthorViewModel();
        BindingContext = vm;
    }

    public AuthorSearchCPage(AuthorViewModel author, AuthorSearchCViewModel vm)
    {
        InitializeComponent();
		vm.Author = author;
        BindingContext = vm;
    }
}
