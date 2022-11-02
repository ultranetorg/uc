namespace UC.Umc.Pages;

public partial class AuthorSearchCPage : CustomPage
{
    public AuthorSearchCPage()
    {
        InitializeComponent();
        var vm = Ioc.Default.GetService<AuthorSearchCViewModel>();
		vm.Author = new Author();
        BindingContext = vm;
    }

    public AuthorSearchCPage(Author author, AuthorSearchCViewModel vm)
    {
        InitializeComponent();
		vm.Author = author;
        BindingContext = vm;
    }
}
