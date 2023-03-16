namespace UO.Mobile.UUC.Services.NetworkStatistic;

public class NetworkStatisticMockService : INetworkStatisticService
{
    public Task<Models.NetworkStatistic> GetNetworkStatisticAsync()
    {
        Random random = new();
        int randomSeconds = random.Next(1, 10000);

        Models.NetworkStatistic networkStatistic = new()
        {
            ActiveUsersCount = 94,
            Bandwidth = 18000000,
            CurrentDateTime = DateTime.Now,
            LastBlockDateTime = DateTime.Now.AddSeconds(-randomSeconds),
            NodesCount = 381,
            RoundNumber = 36017,
        };

        return Task.FromResult(networkStatistic);
    }
}
