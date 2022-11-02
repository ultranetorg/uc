namespace UC.Net.Node.MAUI.Pages;

public partial class AuthorSearchBPage : CustomPage
{
    public AuthorSearchBPage()
    {
        InitializeComponent();
        var vm = Ioc.Default.GetService<AuthorSearchBViewModel>();
		vm.Author = new Author();
        BindingContext = vm;
    }

    public AuthorSearchBPage(Author author, AuthorSearchBViewModel vm)
    {
        InitializeComponent();
		vm.Author = author;
        BindingContext = vm;
    }
}
