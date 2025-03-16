namespace Uccs.Fair;

public class ReviewTable : Table<ReviewEntry>
{
	public IEnumerable<FairRound>	Tail => Mcv.Tail.Cast<FairRound>();
	public new FairMcv				Mcv => base.Mcv as FairMcv;

	public ReviewTable(FairMcv rds) : base(rds)
	{
	}
	
	public override ReviewEntry Create()
	{
		return new ReviewEntry(Mcv);
	}
 }
