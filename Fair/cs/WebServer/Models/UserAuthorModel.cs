using System.Text.Json.Serialization;

namespace Uccs.Fair;

public class UserAuthorModel : AuthorBaseModel
{
	public IEnumerable<string> OwnersIds { get; set; }

	public long ModerationReward { get; set; }

	public short Expiration { get; set; }

	public long Space { get; set; }
	public long Spacetime { get; set; }

	public long Energy { get; set; }
	public byte EnergyThisPeriod { get; set; }
	public long EnergyNext { get; set; }

	public long Bandwidth { get; set; }
	public short BandwidthExpiration { get; set; }
	public short BandwidthTodayTime { get; set; }
	public long BandwidthTodayAvailable { get; set; }

	public UserAuthorModel(Author author) : base(author)
	{
		OwnersIds = author.Owners.Select(x => x.ToString());
		ModerationReward = author.ModerationReward;
		Expiration = author.Expiration;
		Space = author.Space;
		Energy = author.Energy;
		EnergyThisPeriod = author.EnergyThisPeriod;
		EnergyNext = author.EnergyNext;
		Bandwidth = author.Bandwidth;
		BandwidthExpiration = author.BandwidthExpiration;
		BandwidthTodayAvailable = author.BandwidthTodayBalance;
	}
}
