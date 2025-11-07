namespace UC.Umc.Services;

public class AuthorsMockService : IAuthorsService
{
    private readonly IServicesMockData _service;

    public AuthorsMockService(IServicesMockData mockServiceData)
    {
        _service = mockServiceData;
    }

    public CustomCollection<AuthorViewModel> GetAccountAuthors(string account)
	{
		var authors = _service.Authors.Where(x => x.Account.Address == account).ToList();
		return new CustomCollection<AuthorViewModel>(authors);
	}

    public Task<ObservableCollection<AuthorViewModel>> GetAuthorsAsync() =>
		Task.FromResult(new ObservableCollection<AuthorViewModel>(_service.Authors));

    public async Task<ObservableCollection<AuthorViewModel>> SearchAuthorsAsync(string search)
    {
		var items = _service.Authors.Where(x => x.Name.Contains(search)).ToList();
        var result = new ObservableCollection<AuthorViewModel>(items);
        return await Task.FromResult(result);
    }

    public async Task<ObservableCollection<AuthorViewModel>> FilterAuthorsAsync(AuthorStatus status) =>
		await Task.FromResult(new ObservableCollection<AuthorViewModel>(
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
