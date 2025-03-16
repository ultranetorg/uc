namespace Uccs.Fair;

public class SiteTable : Table<SiteEntry>
{
	public IEnumerable<FairRound>	Tail => Mcv.Tail.Cast<FairRound>();
	public new FairMcv				Mcv => base.Mcv as FairMcv;

	public SiteTable(FairMcv rds) : base(rds)
	{
	}
	
	public override SiteEntry Create()
	{
		return new SiteEntry(Mcv);
	}
 }
