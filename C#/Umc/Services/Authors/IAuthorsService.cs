namespace UC.Umc.Services;

public interface IAuthorsService
{
    Task<ObservableCollection<AuthorViewModel>> GetAccountAuthorsAsync();

    Task<ObservableCollection<AuthorViewModel>> GetAuctionAuthorsAsync();

    Task<ObservableCollection<AuthorViewModel>> SearchAuthorsAsync();

	Task RegisterAuthorAsync(AuthorViewModel author);

	Task RenewAuthorAsync(AuthorViewModel author);

	Task MakeBidAsync(AuthorViewModel author, decimal amount);
}
