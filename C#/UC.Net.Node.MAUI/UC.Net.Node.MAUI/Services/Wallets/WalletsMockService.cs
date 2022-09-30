namespace UC.Net.Node.MAUI.Services;

public class WalletsMockService : IWalletsService
{
    private readonly IServicesMockData _data;

    public WalletsMockService(IServicesMockData mockServiceData)
    {
        _data = mockServiceData;
    }

    public Task<int> GetCountAsync()
    {
        int result = _data.Authors.Count;
        return Task.FromResult(result);
    }

    public Task<ObservableCollection<Wallet>> GetAllAsync()
    {
        ObservableCollection<Wallet> result = new(_data.Wallets);
        return Task.FromResult(result);
    }
}
