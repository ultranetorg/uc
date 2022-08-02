namespace UC.Net.Node.MAUI.Pages;

public partial class AuthorSearchCPage : CustomPage
{
    public AuthorSearchCPage(Author author)
    {
        InitializeComponent();
        BindingContext = new AuthorSearchCViewModel(author, ServiceHelper.GetService<ILogger<AuthorSearchCViewModel>>());
    }
}
