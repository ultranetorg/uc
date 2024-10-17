using UO.DomainModels.Operations;

namespace UC.DomainModels;

public class AccountModel
{
	public string Id { get; set; } = null!;

	public string Address { get; init; } = null!;

	public long BYBalance { get; set; }
	public long ECBalance { get; set; }
	public int ECReservationsCount { get; set; }

	public int LastTransactionNid { get; set; } = -1;

	public long AverageUptime { get; set; }

	public long BandwidthNext { get; set; }
	public int BandwidthExpiration { get; set; }
	public long BandwidthToday { get; set; }
	public int BandwidthTodayTime { get; set; }
	public long BandwidthTodayAvailable { get; set; }

	public IEnumerable<AccountECReservation> ECReservations { get; set; } = null!;

	public IEnumerable<DomainModel> Domains { get; set; } = null!;

	public IEnumerable<BaseOperationModel> Operations { get; set; } = null!;
}
