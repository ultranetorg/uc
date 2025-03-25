namespace Uccs.Fair;

public class SiteTable : Table<Site>
{
	public IEnumerable<FairRound>	Tail => Mcv.Tail.Cast<FairRound>();
	public new FairMcv				Mcv => base.Mcv as FairMcv;

	public SiteTable(FairMcv rds) : base(rds)
	{
	}
	
	public override Site Create()
	{
		return new Site(Mcv);
	}
 }
