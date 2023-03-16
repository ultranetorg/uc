using UO.Mobile.UUC.Services.NetworkStatistic;
using UO.Mobile.UUC.ViewModels.Base;

namespace UO.Mobile.UUC.ViewModels;

public class NetworkViewModel : BaseViewModel
{
    private int _nodesCount;
    private int _activeUsersCount;
    private int _bandwidth;
    private DateTime _lastBlockDateTime;
    private int _roundNumber;
    private DateTime _currentDateTime;

    private readonly INetworkStatisticService _networkStatisticService;

    public NetworkViewModel(INetworkStatisticService networkStatisticService)
    {
        _networkStatisticService = networkStatisticService;

        Fetch();
    }

    private async void Fetch()
    {
        Models.NetworkStatistic networkStatistic = await _networkStatisticService.GetNetworkStatisticAsync();
        UpdateNetworkStatistic(networkStatistic);
    }

    private void UpdateNetworkStatistic(Models.NetworkStatistic networkStatistic)
    {
        NodesCount = networkStatistic.NodesCount;
        ActiveUsersCount = networkStatistic.ActiveUsersCount;
        Bandwidth = networkStatistic.Bandwidth;
        LastBlockDateTime = networkStatistic.LastBlockDateTime;
        RoundNumber = networkStatistic.RoundNumber;
        CurrentDateTime = networkStatistic.CurrentDateTime;
    }

    public int NodesCount
    {
        get => _nodesCount;
        set
        {
            _nodesCount = value;
            OnPropertyChanged(nameof(NodesCount));
        }
    }

    public int ActiveUsersCount
    {
        get => _activeUsersCount;
        set
        {
            _activeUsersCount = value;
            OnPropertyChanged(nameof(ActiveUsersCount));
        }
    }

    public int Bandwidth
    {
        get => _bandwidth;
        set
        {
            _bandwidth = value;
            OnPropertyChanged(nameof(Bandwidth));
        }
    }

    public DateTime LastBlockDateTime
    {
        get => _lastBlockDateTime;
        set
        {
            _lastBlockDateTime = value;
            OnPropertyChanged(nameof(LastBlockDateTime));
        }
    }

    public int RoundNumber
    {
        get => _roundNumber;
        set
        {
            _roundNumber = value;
            OnPropertyChanged(nameof(RoundNumber));
        }
    }

    public DateTime CurrentDateTime
    {
        get => _currentDateTime;
        set
        {
            _currentDateTime = value;
            OnPropertyChanged(nameof(CurrentDateTime));
        }
    }
}
