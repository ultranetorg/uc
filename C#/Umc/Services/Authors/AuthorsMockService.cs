namespace UC.Umc.Services;

public class AuthorsMockService : IAuthorsService
{
    private readonly IServicesMockData _service;

    public AuthorsMockService(IServicesMockData mockServiceData)
    {
        _service = mockServiceData;
    }

    public Task<ObservableCollection<AuthorViewModel>> GetAccountAuthorsAsync() =>
		Task.FromResult(new ObservableCollection<AuthorViewModel>(_service.Authors));

    public Task<ObservableCollection<AuthorViewModel>> SearchAuthorsAsync(string search)
    {
		var items = _service.Authors.Where(x => x.Name.Contains(search)).ToList();
        var result = new ObservableCollection<AuthorViewModel>(items);
        return Task.FromResult(result);
    }

    public Task<ObservableCollection<AuthorViewModel>> FilterAuthorsAsync(AuthorStatus status) =>
		Task.FromResult(new ObservableCollection<AuthorViewModel>(
			_service.Authors.Where(x => x.Status == status)));

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
