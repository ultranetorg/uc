namespace UC.Net.Node.MAUI.Pages;

public partial class AuthorSearchBPage : CustomPage
{
    public AuthorSearchBPage(Author author, AuthorSearchBViewModel vm)
    {
        InitializeComponent();
		vm.Author = author;
        BindingContext = vm;
    }
}
