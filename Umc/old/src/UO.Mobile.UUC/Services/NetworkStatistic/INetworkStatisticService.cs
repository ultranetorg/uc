namespace UO.Mobile.UUC.Services.NetworkStatistic;

public interface INetworkStatisticService
{
    Task<Models.NetworkStatistic> GetNetworkStatisticAsync();
}
