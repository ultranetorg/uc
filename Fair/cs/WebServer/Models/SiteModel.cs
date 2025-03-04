namespace Uccs.Fair;

public class SiteModel : SiteBaseModel
{
	public int ModerationReward { get; set; }
	public ElectionPolicy ModerationPermissions { get; set; }

	public short Expiration { get; set; }
	public long Space { get; set; }
	public long Spacetime { get; set; }

	public long Energy { get; set; }
	public byte EnergyThisPeriod { get; set; }
	public long EnergyNext { get; set; }
	public long Bandwidth { get; set; }
	public short BandwidthExpiration { get; set; } = -1;
	public long BandwidthToday { get; set; }
	public short BandwidthTodayTime { get; set; }
	public long BandwidthTodayAvailable { get; set; }

	public IEnumerable<AuthorBaseModel> Authors { get; set; }

	public IEnumerable<AccountModel> Moderators { get; set; }

	public IEnumerable<CategoryBaseModel> Categories { get; set; }

	// public IEnumerable<object> Disputes { get; set; }

	public SiteModel(Site site) : base(site)
	{
		ModerationReward = site.ModerationReward;
		ModerationPermissions = site.ModeratorElectionPolicy;
		Expiration = site.Expiration;
		Space = site.Space;
		Spacetime = site.Spacetime;
		Energy = site.Energy;
		EnergyThisPeriod = site.EnergyThisPeriod;
		EnergyNext = site.EnergyNext;
		Bandwidth = site.Bandwidth;
		BandwidthExpiration = site.BandwidthExpiration;
		BandwidthToday = site.BandwidthToday;
		BandwidthTodayTime = site.BandwidthTodayTime;
		BandwidthTodayAvailable = site.BandwidthTodayAvailable;
	}
}
