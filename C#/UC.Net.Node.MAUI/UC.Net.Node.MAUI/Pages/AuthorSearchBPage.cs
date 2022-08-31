namespace UC.Net.Node.MAUI.Pages;

public partial class AuthorSearchBPage : CustomPage
{
    public AuthorSearchBPage()
    {
        InitializeComponent();
        var vm = App.ServiceProvider.GetService<AuthorSearchBViewModel>();
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
