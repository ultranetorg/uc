namespace UC.Net.Node.MAUI.Pages;

public partial class AuthorSearchBPage : CustomPage
{
    public AuthorSearchBPage(Author author)
    {
        InitializeComponent();
        BindingContext = new AuthorSearchBViewModel(author, ServiceHelper.GetService<ILogger<AuthorSearchBViewModel>>());
    }
}
