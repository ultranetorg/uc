using UO.Mobile.UUC.Models;

namespace UO.Mobile.UUC.Services.Authors;

public interface IAuthorsService
{
    Task<int> GetCountAsync();

    Task<ObservableCollection<Author>> GetAllAsync();
}
