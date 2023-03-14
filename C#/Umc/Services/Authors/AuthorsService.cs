namespace UC.Umc.Services;

public sealed class AuthorsService : IAuthorsService
{
	public Task<ObservableCollection<AuthorViewModel>> GetAccountAuthorsAsync()
	{
		throw new NotImplementedException();
	}

	public Task<ObservableCollection<AuthorViewModel>> SearchAuthorsAsync(string search)
	{
		throw new NotImplementedException();
	}

	public Task<ObservableCollection<AuthorViewModel>> FilterAuthorsAsync(AuthorStatus status)
	{
		throw new NotImplementedException();
	}

	public Task MakeBidAsync(AuthorViewModel author, decimal amount)
	{
		throw new NotImplementedException();
	}

	public Task RegisterAuthorAsync(AuthorViewModel author)
	{
		throw new NotImplementedException();
	}

	public Task RenewAuthorAsync(AuthorViewModel author)
	{
		throw new NotImplementedException();
	}
}
