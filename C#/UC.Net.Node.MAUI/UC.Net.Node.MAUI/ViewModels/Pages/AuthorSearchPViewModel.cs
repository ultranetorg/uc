namespace UC.Net.Node.MAUI.ViewModels.Pages;

public partial class AuthorSearchPViewModel : BaseAuthorViewModel
{
    public AuthorSearchPViewModel(Author author, ILogger<AuthorSearchPViewModel> logger) : base(logger)
    {
        Author = author;
    }
}
