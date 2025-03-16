namespace Uccs.Fair;

public class DisputeTable : Table<DisputeEntry>
{
	public IEnumerable<FairRound>	Tail => Mcv.Tail.Cast<FairRound>();
	public new FairMcv				Mcv => base.Mcv as FairMcv;

	public DisputeTable(FairMcv rds) : base(rds)
	{
	}
	
	public override DisputeEntry Create()
	{
		return new DisputeEntry(Mcv);
	}
 }
