namespace UC.Net.Node.MAUI.Services;

public interface IWalletsService
{
    Task<int> GetCountAsync();

    Task<ObservableCollection<Wallet>> GetAllAsync();
}
