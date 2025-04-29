namespace Uccs.Fair;

public class DisputeTable : Table<AutoId, Dispute>
{
	public IEnumerable<FairRound>	Tail => Mcv.Tail.Cast<FairRound>();
	public new FairMcv				Mcv => base.Mcv as FairMcv;

	public DisputeTable(FairMcv rds) : base(rds)
	{
	}
	
	public override Dispute Create()
	{
		return new Dispute(Mcv);
	}
 }
