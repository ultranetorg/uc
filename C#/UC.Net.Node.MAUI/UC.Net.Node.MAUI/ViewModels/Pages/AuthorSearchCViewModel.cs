namespace UC.Net.Node.MAUI.ViewModels.Pages;

public partial class AuthorSearchCViewModel : BaseAuthorViewModel
{
    public AuthorSearchCViewModel(Author author, ILogger<AuthorSearchCViewModel> logger) : base(logger)
    {
        Author = author;
    }
}
