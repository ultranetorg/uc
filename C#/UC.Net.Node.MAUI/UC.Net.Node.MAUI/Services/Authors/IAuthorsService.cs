namespace UC.Net.Node.MAUI.Services;

public interface IAuthorsService
{
    Task<int> GetCountAsync();

    Task<ObservableCollection<Author>> GetAllAsync();
}
