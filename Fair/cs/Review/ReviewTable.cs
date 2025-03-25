namespace Uccs.Fair;

public class ReviewTable : Table<Review>
{
	public IEnumerable<FairRound>	Tail => Mcv.Tail.Cast<FairRound>();
	public new FairMcv				Mcv => base.Mcv as FairMcv;

	public ReviewTable(FairMcv rds) : base(rds)
	{
	}
	
	public override Review Create()
	{
		return new Review(Mcv);
	}
 }
