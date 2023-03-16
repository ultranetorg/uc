namespace UO.Mobile.UUC.Models;

public class NetworkStatistic
{
    public int NodesCount { get; set; }

    public int ActiveUsersCount { get; set; }

    public int Bandwidth { get; set; }

    public DateTime LastBlockDateTime { get; set; }

    public int RoundNumber { get; set; }

    public DateTime CurrentDateTime { get; set; }
}
