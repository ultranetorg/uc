namespace UC.Net.Node.MAUI.Pages;

public partial class AuthorSearchPage : CustomPage
{
    public AuthorSearchPage(Author author)
    {
        InitializeComponent();
        BindingContext = new AuthorSearchPViewModel(author, ServiceHelper.GetService<ILogger<AuthorSearchPViewModel>>());
    }
}
