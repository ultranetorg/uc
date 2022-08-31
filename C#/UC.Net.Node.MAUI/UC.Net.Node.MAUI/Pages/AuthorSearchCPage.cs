namespace UC.Net.Node.MAUI.Pages;

public partial class AuthorSearchCPage : CustomPage
{
    public AuthorSearchCPage()
    {
        InitializeComponent();
        var vm = App.ServiceProvider.GetService<AuthorSearchCViewModel>();
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
