namespace UC.Umc.Services;

public class AuthorsMockService : IAuthorsService
{
    private readonly IServicesMockData _data;

    public AuthorsMockService(IServicesMockData mockServiceData)
    {
        _data = mockServiceData;
    }

    public Task<int> GetCountAsync()
    {
        int result = _data.Authors.Count;
        return Task.FromResult(result);
    }

    public Task<ObservableCollection<Author>> GetAllAsync()
    {
        var result = new ObservableCollection<Author>(_data.Authors);
        return Task.FromResult(result);
    }
}
