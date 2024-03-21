namespace UC.Umc.Services;

public interface IAuthorsService
{
    Task<ObservableCollection<AuthorViewModel>> GetAccountAuthorsAsync();

    Task<ObservableCollection<AuthorViewModel>> SearchAuthorsAsync(string search);

    Task<ObservableCollection<AuthorViewModel>> FilterAuthorsAsync(AuthorStatus status);

	Task RegisterAuthorAsync(AuthorViewModel author);

	Task RenewAuthorAsync(AuthorViewModel author);

	Task MakeBidAsync(AuthorViewModel author, decimal amount);
}
