namespace UC.Umc.Services;

public interface IAuthorsService
{
    Task<int> GetCountAsync();

    Task<ObservableCollection<Author>> GetAllAsync();
}
