namespace UC.Net.Node.MAUI.Services.Authors;

public interface IAuthorsService
{
    Task<int> GetCountAsync();

    Task<ObservableCollection<Author>> GetAllAsync();
}
