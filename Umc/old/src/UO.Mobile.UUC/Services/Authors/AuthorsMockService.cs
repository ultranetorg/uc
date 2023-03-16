using UO.Mobile.UUC.Models;

namespace UO.Mobile.UUC.Services.Authors;

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
        ObservableCollection<Author> result = new(_data.Authors);
        return Task.FromResult(result);
    }
}
