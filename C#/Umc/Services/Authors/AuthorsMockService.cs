namespace UC.Umc.Services;

public class AuthorsMockService : IAuthorsService
{
    private readonly IServicesMockData _data;

    public AuthorsMockService(IServicesMockData mockServiceData)
    {
        _data = mockServiceData;
    }

    public Task<ObservableCollection<AuthorViewModel>> GetAccountAuthorsAsync()
    {
        var result = new ObservableCollection<AuthorViewModel>(_data.Authors);
        return Task.FromResult(result);
    }

    public Task<ObservableCollection<AuthorViewModel>> GetAuctionAuthorsAsync()
    {
        var result = new ObservableCollection<AuthorViewModel>(_data.Authors);
        return Task.FromResult(result);
    }

    public Task<ObservableCollection<AuthorViewModel>> SearchAuthorsAsync()
    {
        var result = new ObservableCollection<AuthorViewModel>(_data.Authors);
        return Task.FromResult(result);
    }

	public Task RegisterAuthorAsync(AuthorViewModel author)
	{
		throw new NotImplementedException();
	}

	public Task RenewAuthorAsync(AuthorViewModel author)
	{
		throw new NotImplementedException();
	}

	public Task MakeBidAsync(AuthorViewModel author, decimal amount)
	{
		throw new NotImplementedException();
	}
}
