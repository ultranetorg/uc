namespace UC.Net.Node.MAUI.Pages;

public partial class AuthorSearchCPage : CustomPage
{
    public AuthorSearchCPage(Author author, AuthorSearchCViewModel vm)
    {
        InitializeComponent();
		vm.Author = author;
        BindingContext = vm;
    }
}
