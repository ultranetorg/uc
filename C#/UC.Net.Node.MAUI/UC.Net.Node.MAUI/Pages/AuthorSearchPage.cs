namespace UC.Net.Node.MAUI.Pages;

public partial class AuthorSearchPage : CustomPage
{
    public AuthorSearchPage(Author author, AuthorSearchPViewModel vm)
    {
        InitializeComponent();
		vm.Author = author;
        BindingContext = vm;
    }

    public AuthorSearchPage(Author author)
    {
        InitializeComponent();
		var vm = App.ServiceProvider.GetService<AuthorSearchPViewModel>();
		vm.Author = author;
        BindingContext = vm;
    }

	public AuthorSearchPage()
	{
        InitializeComponent();
		var vm = App.ServiceProvider.GetService<AuthorSearchPViewModel>();
		vm.Author = new Author();
        BindingContext = vm;
	}
}
